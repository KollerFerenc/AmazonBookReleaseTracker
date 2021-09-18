using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Serilog;
using CommandDotNet;

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
        public void AddLink(AmazonLink amazonLink)
        {
            if (!Config.SettingsImported)
            {
                Config.ImportSettings();
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                switch (amazonLink.GetProductType())
                {
                    case AmazonProductType.Book:
                        if (Config.Settings.TrackedBooks.Contains(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } book already tracked.");
                        }
                        else
                        {
                            Config.Settings.TrackedBooks.Add(amazonId);
                            Log.Information($"{ amazonId.Asin } book tracked.");
                            Config.SaveConfig();
                        }
                        break;
                    case AmazonProductType.Series:
                        if (Config.Settings.TrackedSeries.Contains(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } series already tracked.");
                        }
                        else
                        {
                            Config.Settings.TrackedSeries.Add(amazonId);
                            Log.Information($"{ amazonId.Asin } series tracked.");
                            Config.SaveConfig();
                        }
                        break;
                    case AmazonProductType.Unknown:
                        Log.Error("Could not determine product type.");
                        Program.Exit(ExitCode.NoProductType);
                        break;
                    default:
                        Log.Error("Could not determine product type.");
                        Program.Exit(ExitCode.NoProductType);
                        break;
                }
            }
            else
            {
                Log.Error("Could not find id.");
                Program.Exit(ExitCode.NoIdFound);
            }
        }

        [Command(
            Name = "remove",
            Description = "Remove tracking from link.")]
        public void RemoveLink(AmazonLink amazonLink)
        {
            if (!Config.SettingsImported)
            {
                Config.ImportSettings();
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                switch (amazonLink.GetProductType())
                {
                    case AmazonProductType.Book:
                        if (Config.Settings.TrackedBooks.Remove(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } removed book tracking.");
                            Config.SaveConfig();
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
                            Config.SaveConfig();
                        }
                        else
                        {
                            Log.Error($"Could not find { amazonId.Asin }.");
                        }
                        break;
                    case AmazonProductType.Unknown:
                        Log.Error("Could not determine product type.");
                        Program.Exit(ExitCode.NoProductType);
                        break;
                    default:
                        Log.Error("Could not determine product type.");
                        Program.Exit(ExitCode.NoProductType);
                        break;
                }
            }
            else
            {
                Log.Error("Could not find id.");
                Program.Exit(ExitCode.NoIdFound);
            }
        }

        [Command(
            Name = "ignore",
            Description = "Ignore this id.")]
        public void IgnoreLink(
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
                Config.ImportSettings();
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                if (remove)
                {
                    if (Config.Settings.IgnoredIds.Remove(amazonId))
                    {
                        Log.Information($"{ amazonId.Asin } removed from ignore list.");
                        Config.SaveConfig();
                    }
                    else
                    {
                        Log.Error($"Could not find { amazonId.Asin }.");
                    }
                }
                else
                {
                    if (Config.Settings.IgnoredIds.Contains(amazonId))
                    {
                        Log.Information($"{ amazonId.Asin } asin already in ignore list.");
                    }
                    else
                    {
                        Config.Settings.IgnoredIds.Add(amazonId);
                        Log.Information($"{ amazonId.Asin } asin added to ignore list.");
                        Config.SaveConfig();
                    }
                }
            }
            else
            {
                Log.Error("Could not find id.");
                Program.Exit(ExitCode.NoIdFound);
            }
        }

        [Command(
            Name = "run",
            Description = "Run tracking.")]
        public async Task Run()
        {
            if (!Config.SettingsImported)
            {
                Config.ImportSettings();
            }

            var dateNow = DateTime.Now;
            var seriesIds = Config.Settings.TrackedSeries;
            var booksIds = Config.Settings.TrackedBooks;

            Log.Debug("Removing ignored series.");
            foreach (var item in Config.Settings.IgnoredIds)
            {
                seriesIds.Remove(item);
            }

            Log.Information($"Processing { seriesIds.Count } series.");
            var amazonSeriesList = new List<AmazonSeries>(seriesIds.Count);
            foreach (var seriesId in seriesIds)
            {
                var tempSeries = new AmazonSeries(seriesId);
                var html = await Utilities.GetHtml(tempSeries.GetUri());

                try
                {
                    tempSeries.ProcessHtml(html);

                    Log.Debug("Removing ignored books from series.");
                    tempSeries.Books = tempSeries.Books.FindAll(b => !Config.Settings.IgnoredIds.Contains(b.AmazonId));

                    amazonSeriesList.Add(tempSeries);
                }
                catch (NotSeriesException)
                {
                    Log.Error($"Skipping { seriesId.Asin }, it is not a series.");
                }
            }

            Log.Debug("Removing ignored books.");
            foreach (var item in Config.Settings.IgnoredIds)
            {
                booksIds.Remove(item);
            }

            int count = 0;
            foreach (var series in amazonSeriesList)
            {
                count += series.Books.Count;
            }

            var oldReleasIds = new List<AmazonId>();

            Log.Information($"Processing { count } + { booksIds.Count } books.");
            foreach (var series in amazonSeriesList)
            {
                foreach (var book in series.Books)
                {
                    var html = await Utilities.GetHtml(book.GetUri());
                    try
                    {
                        book.ProcessHtml(html);

                        if (Config.Settings.IgnoreReleasedBooks)
                        {
                            if (book.ReleaseDate.AddDays((double)Config.Settings.IgnoreAfterReleaseDays) < dateNow)
                            {
                                oldReleasIds.Add(book.AmazonId);
                            }
                        }
                    }
                    catch (NotBookException)
                    {
                        Log.Error($"Skipping { book.AmazonId.Asin }, it is not a book.");
                    }
                }
            }

            var amazonBooksList = new List<AmazonBook>(booksIds.Count);
            foreach (var bookId in booksIds)
            {
                var tempBook = new AmazonBook(bookId);

                var html = await Utilities.GetHtml(tempBook.GetUri());
                try
                {
                    tempBook.ProcessHtml(html);

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
                catch (NotBookException)
                {
                    Log.Error($"Skipping { tempBook.AmazonId.Asin }, it is not a book.");
                }
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

                Config.SaveConfig();
            }

            if (File.Exists(Utilities.pathToDataNew))
            {
                Log.Verbose("Moving old data file.");
                File.Move(Utilities.pathToDataNew, Utilities.pathToDataOld, overwrite: true);
            }

            var trackingData = new TrackingData(dateNow, amazonSeriesList, amazonBooksList);

            Log.Debug("Writing new data file.");
            using (var writer = new StreamWriter(Utilities.pathToDataNew, append: false))
            {
                string json = JsonSerializer.Serialize(trackingData, Utilities.jsonSerializerOptions);
                writer.Write(json);
            }
        }
    }
}
