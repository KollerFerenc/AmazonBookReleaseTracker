using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Serilog;
using CommandDotNet;
using System.Linq;
using System.Threading;

namespace AmazonBookReleaseTracker
{
    public class AmazonBookReleaseTracker
    {
        [SubCommand]
        public Export Export { get; private set; } = new();
        [SubCommand]
        public Config Config { get; private set; } = new();

        [Command(
            Name = "add",
            Description = "Add id from amazon link.")]
        public int AddLink(AmazonLink amazonLink)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                switch (amazonLink.GetProductType())
                {
                    case AmazonProductType.Book:
                        if (Config.Settings.TrackedBooks.Add(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } book tracked.");
                            Config.SaveConfig(Utilities.pathToConfig);
                        }
                        else
                        {
                            Log.Information($"{ amazonId.Asin } book already tracked.");
                        }
                        break;
                    case AmazonProductType.Series:
                        if (Config.Settings.TrackedSeries.Add(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } series tracked.");
                            Config.SaveConfig(Utilities.pathToConfig);
                        }
                        else
                        {
                            Log.Information($"{ amazonId.Asin } series already tracked.");
                        }
                        break;
                    case AmazonProductType.Unknown:
                        Log.Error("Could not determine product type.");
                        return (int)ExitCode.NoProductType;
                    default:
                        Log.Error("Could not determine product type.");
                        return (int)ExitCode.NoProductType;
                }
            }
            else
            {
                Log.Error("Could not find id.");
                return (int)ExitCode.NoIdFound;
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "remove",
            Description = "Remove tracking from link.")]
        public int RemoveLink(AmazonLink amazonLink)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                switch (amazonLink.GetProductType())
                {
                    case AmazonProductType.Book:
                        if (Config.Settings.TrackedBooks.Remove(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } removed book tracking.");
                            Config.SaveConfig(Utilities.pathToConfig);
                        }
                        else
                        {
                            Log.Error($"Could not find { amazonId.Asin }.");
                        }
                        break;
                    case AmazonProductType.Series:
                        if (Config.Settings.TrackedSeries.Remove(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } removed series tracking.");
                            Config.SaveConfig(Utilities.pathToConfig);
                        }
                        else
                        {
                            Log.Error($"Could not find { amazonId.Asin }.");
                        }
                        break;
                    case AmazonProductType.Unknown:
                        Log.Error("Could not determine product type.");
                        return (int)ExitCode.NoProductType;
                    default:
                        Log.Error("Could not determine product type.");
                        return (int)ExitCode.NoProductType;
                }
            }
            else
            {
                Log.Error("Could not find id.");
                return (int)ExitCode.NoIdFound;
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "ignore",
            Description = "Ignore this id.")]
        public int IgnoreLink(
            AmazonLink amazonLink,
            [Option(
                LongName = "remove",
                ShortName = "r",
                Description="Remove this id from ignored list.",
                BooleanMode=BooleanMode.Implicit)]
            bool remove)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                if (remove)
                {
                    if (Config.Settings.IgnoredIds.Remove(amazonId))
                    {
                        Log.Information($"{ amazonId.Asin } removed from ignore list.");
                        Config.SaveConfig(Utilities.pathToConfig);
                    }
                    else
                    {
                        Log.Error($"Could not find { amazonId.Asin }.");
                    }
                }
                else
                {
                    if (Config.Settings.IgnoredIds.Add(amazonId))
                    {
                        Log.Information($"{ amazonId.Asin } asin added to ignore list.");
                        Config.SaveConfig(Utilities.pathToConfig);
                    }
                    else
                    {
                        Log.Information($"{ amazonId.Asin } asin already in ignore list.");
                    }
                }
            }
            else
            {
                Log.Error("Could not find id.");
                return (int)ExitCode.NoIdFound;
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "run",
            Description = "Run tracking.")]
        public async Task<int> RunTrackingAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var dateNow = DateTime.Now;
            var seriesIds = Config.Settings.TrackedSeries;
            var booksIds = Config.Settings.TrackedBooks;

            var oldReleasIds = new SortedSet<AmazonId>(new AmazonIdComparer());

            Log.Debug("Removing ignored series.");
            seriesIds.ExceptWith(Config.Settings.IgnoredIds);

            cancellationToken.ThrowIfCancellationRequested();
            Log.Information($"Processing { seriesIds.Count } series.");
            var amazonSeriesList = new List<AmazonSeries>(seriesIds.Count);
            foreach (var seriesId in seriesIds)
            {
                var tempSeries = new AmazonSeries(seriesId);
                var html = await Utilities.GetHtml(tempSeries.GetUri(), cancellationToken);

                try
                {
                    if (html is null)
                    {
                        Log.Error($"Page for { seriesId.Asin } not available. Adding to ignore list.");
                        oldReleasIds.Add(tempSeries.AmazonId);
                    }
                    else if (tempSeries.ProcessHtml(html))
                    {
                        Log.Debug("Removing ignored books from series.");
                        tempSeries.Books = tempSeries.Books.FindAll(b => !Config.Settings.IgnoredIds.Contains(b.AmazonId));

                        amazonSeriesList.Add(tempSeries);
                    }
                    else
                    {
                        Log.Error($"Cannot parse { tempSeries.Title } ({ tempSeries.AmazonId.Asin }).");
                    }
                }
                catch (NotSeriesException)
                {
                    Log.Error($"Skipping { seriesId.Asin }, it is not a series.");
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            Log.Debug("Removing ignored books.");
            booksIds.ExceptWith(Config.Settings.IgnoredIds);

            int count = amazonSeriesList.Sum(x => x.Books.Count);

            cancellationToken.ThrowIfCancellationRequested();
            Log.Information($"Processing { count } + { booksIds.Count } books.");
            foreach (var series in amazonSeriesList)
            {
                foreach (var book in series.Books)
                {
                    var html = await Utilities.GetHtml(book.GetUri(), cancellationToken);
                    try
                    {
                        if (html is null)
                        {
                            Log.Error($"Page for { book.AmazonId.Asin } not available. Adding to ignore list.");
                            oldReleasIds.Add(book.AmazonId);
                        }
                        else if (book.ProcessHtml(html))
                        {
                            if (Config.Settings.IgnoreReleasedBooks)
                            {
                                if (book.ReleaseDate.AddDays((double)Config.Settings.IgnoreAfterReleaseDays) < dateNow)
                                {
                                    oldReleasIds.Add(book.AmazonId);
                                }
                            }
                        }
                        else
                        {
                            Log.Error($"Cannot parse { book.Title } ({ book.AmazonId.Asin }).");
                        }
                    }
                    catch (NotBookException)
                    {
                        Log.Error($"Skipping { book.AmazonId.Asin }, it is not a book.");
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            var amazonBooksList = new List<AmazonBook>(booksIds.Count);
            foreach (var bookId in booksIds)
            {
                var tempBook = new AmazonBook(bookId);

                var html = await Utilities.GetHtml(tempBook.GetUri(), cancellationToken);
                try
                {
                    if (html is null)
                    {
                        Log.Error($"Page for { tempBook.AmazonId.Asin } not available. Adding to ignore list.");
                        oldReleasIds.Add(tempBook.AmazonId);
                    }
                    else if (tempBook.ProcessHtml(html))
                    {
                        if (Config.Settings.IgnoreReleasedBooks)
                        {
                            if (tempBook.ReleaseDate.AddDays((double)Config.Settings.IgnoreAfterReleaseDays) < dateNow)
                            {
                                oldReleasIds.Add(tempBook.AmazonId);
                            }
                            else
                            {
                                amazonBooksList.Add(tempBook);
                            }
                        }
                        else
                        {
                            amazonBooksList.Add(tempBook);
                        }
                    }
                    else
                    {
                        Log.Error($"Cannot parse { tempBook.Title } ({ tempBook.AmazonId.Asin }).");
                    }
                }
                catch (NotBookException)
                {
                    Log.Error($"Skipping { tempBook.AmazonId.Asin }, it is not a book.");
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            if (oldReleasIds.Count > 0)
            {
                Log.Debug("Removing old books and adding to ignore list.");
                foreach (var series in amazonSeriesList)
                {
                    series.Books = series.Books.FindAll(b => !oldReleasIds.Contains(b.AmazonId));
                }

                foreach (var item in oldReleasIds)
                {
                    Config.Settings.IgnoredIds.Add(item);
                }

                Config.SaveConfig(Utilities.pathToConfig);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(Utilities.pathToDataNew))
            {
                Log.Verbose("Moving old data file.");
                File.Move(Utilities.pathToDataNew, Utilities.pathToDataOld, overwrite: true);
            }

            var trackingData = new TrackingData(dateNow, amazonSeriesList, amazonBooksList);
            Log.Debug("Sorting books by release date.");
            trackingData.SortByReleaseDate();

            Log.Debug("Writing new data file.");
            using (var writer = new StreamWriter(Utilities.pathToDataNew, append: false))
            {
                string json = JsonSerializer.Serialize(trackingData, Utilities.jsonSerializerOptions);
                writer.Write(json);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return (int)ExitCode.Default;
        }

        [Command(
            Name = "createIgnoredData",
            Description = "Create data for ignored books.")]
        public async Task<int> CreateIgnoredData(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var dateNow = DateTime.Now;
            var amazonBooksList = new List<AmazonBook>(Config.Settings.IgnoredIds.Count);
            cancellationToken.ThrowIfCancellationRequested();
            Log.Information($"Processing { Config.Settings.IgnoredIds.Count } books.");
            foreach (var bookId in Config.Settings.IgnoredIds)
            {
                var tempBook = new AmazonBook(bookId);
                var html = await Utilities.GetHtml(tempBook.GetUri(), cancellationToken);

                try
                {
                    if (html is null)
                    {
                        Log.Error($"Skipping { tempBook.AmazonId.Asin }, page not available.");
                    }
                    else if (tempBook.ProcessHtml(html))
                    {
                        amazonBooksList.Add(tempBook);
                    }
                    else
                    {
                        Log.Error($"Cannot parse { tempBook.Title } ({ tempBook.AmazonId.Asin }).");
                    }
                }
                catch (NotBookException)
                {
                    Log.Error($"Skipping { tempBook.AmazonId.Asin }, it is not a book.");
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            var trackingData = new TrackingData(dateNow, Array.Empty<AmazonSeries>(), amazonBooksList);
            Log.Debug("Sorting books by release date.");
            trackingData.SortByReleaseDate();

            Log.Debug("Writing ignored data file.");
            using (var writer = new StreamWriter(Utilities.pathToIgnoredData, append: false))
            {
                string json = JsonSerializer.Serialize(trackingData, Utilities.jsonSerializerOptions);
                writer.Write(json);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return (int)ExitCode.Default;
        }
    }
}
