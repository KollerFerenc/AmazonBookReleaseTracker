using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using System.IO;
using System.Text.Json;
using Serilog;
using CommandDotNet;
using System.Net;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace AmazonBookReleaseTracker
{
    public class AmazonBookReleaseTracker
    {
        private static readonly string _pathToConfig = Path.Combine(Program.baseDirectory, @"config.json");
        private static readonly string _pathToDataNew = Path.Combine(Program.baseDirectory, @"data.new.json");
        private static readonly string _pathToDataOld = Path.Combine(Program.baseDirectory, @"data.old.json");
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new AmazonIdJsonConverter() },
        };
        private static readonly CsvConfiguration _csvConfig = new (CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        private static readonly HttpClientHandler _clientHandler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
        private static readonly HttpClient _client = new(_clientHandler);

        private bool _settingsImported;
        private IAmazonBookReleaseTrackerSettings _settings;

        [Command(
            Name = "init",
            Description = "Initialize config.")]
        public void Init(
            [Option(
                LongName = "force",
                ShortName = "f",
                Description="Overwrite existing configuration.",
                BooleanMode=BooleanMode.Implicit)]
            bool force)
        {
            Log.Information("Initialize config.");

            if (File.Exists(_pathToConfig))
            {
                if (!force)
                {
                    Log.Fatal("Config already exists.");
                    Program.Exit(ExitCode.ConfigExists);
                }
            }

            _settings = new AmazonBookReleaseTrackerSettings();

            _settingsImported = true;
            SaveConfig();
        }

        [Command(
            Name = "showConfig",
            Description = "Show current config.")]
        public void ShowConfig()
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            Log.Debug("Writing config to console.");
            Console.WriteLine(JsonSerializer.Serialize(
                AmazonBookReleaseTrackerSettings.GetSettings(_settings), _jsonSerializerOptions));
        }

        [Command(
            Name = "addSeries",
            Description = "Add series to track.")]
        public void AddSeries(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            if (_settings.TrackedSeries.Contains(amazonId))
            {
                Log.Information($"{ amazonId.Asin } series already tracked.");
            }
            else
            {
                _settings.TrackedSeries.Add(amazonId);
                Log.Information($"{ amazonId.Asin } series tracked.");
                SaveConfig();
            }
        }

        [Command(
            Name = "addBook",
            Description = "Add book to track.")]
        public void AddBook(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            if (_settings.TrackedBooks.Contains(amazonId))
            {
                Log.Information($"{ amazonId.Asin } book already tracked.");
            }
            else
            {
                _settings.TrackedBooks.Add(amazonId);
                Log.Information($"{ amazonId.Asin } book tracked.");
                SaveConfig();
            }
        }

        [Command(
            Name = "addIgnore",
            Description = "Ignore this id.")]
        public void AddIgnore(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            if (_settings.IgnoredIds.Contains(amazonId))
            {
                Log.Information($"{ amazonId.Asin } asin already in ignore list.");
            }
            else
            {
                _settings.IgnoredIds.Add(amazonId);
                Log.Information($"{ amazonId.Asin } asin added to ignore list.");
                SaveConfig();
            }
        }

        [Command(
            Name = "addLink",
            Description = "Add id from amazon link.")]
        public void AddLink(AmazonLink amazonLink)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            if (amazonLink.TryGetAmazonId(out AmazonId amazonId))
            {
                switch (amazonLink.GetProductType())
                {
                    case AmazonProductType.Book:
                        if (_settings.TrackedBooks.Contains(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } book already tracked.");
                        }
                        else
                        {
                            _settings.TrackedBooks.Add(amazonId);
                            Log.Information($"{ amazonId.Asin } book tracked.");
                            SaveConfig();
                        }
                        break;
                    case AmazonProductType.Series:
                        if (_settings.TrackedSeries.Contains(amazonId))
                        {
                            Log.Information($"{ amazonId.Asin } series already tracked.");
                        }
                        else
                        {
                            _settings.TrackedSeries.Add(amazonId);
                            Log.Information($"{ amazonId.Asin } series tracked.");
                            SaveConfig();
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
            Name = "removeSeries",
            Description = "Remove tracking from series.")]
        public void RemoveSeries(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            bool result = _settings.TrackedSeries.Remove(amazonId);

            if (result)
            {
                Log.Information($"{ amazonId.Asin } removed series tracking.");
                SaveConfig();
            }
            else
            {
                Log.Error($"Could not find { amazonId.Asin }.");
            }
        }

        [Command(
            Name = "removeBook",
            Description = "Remove tracking from book.")]
        public void RemoveBook(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            bool result = _settings.TrackedBooks.Remove(amazonId);

            if (result)
            {
                Log.Information($"{ amazonId.Asin } removed book tracking.");
                SaveConfig();
            }
            else
            {
                Log.Error($"Could not find { amazonId.Asin }.");
            }
        }

        [Command(
            Name = "removeIgnore",
            Description = "Remove id from ignored.")]
        public void RemoveIgnore(AmazonId amazonId)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            bool result = _settings.IgnoredIds.Remove(amazonId);

            if (result)
            {
                Log.Information($"{ amazonId.Asin } removed from ignore list.");
                SaveConfig();
            }
            else
            {
                Log.Error($"Could not find { amazonId.Asin }.");
            }
        }

        [Command(
            Name = "removeDuplicates",
            Description = "Remove duplicate ASINs from config.")]
        public void RemoveDuplicates(
            [Option(
                LongName = "dryRun",
                Description= "Dry run.",
                BooleanMode=BooleanMode.Implicit)]
            bool dryRun)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            var distinct = _settings.IgnoredIds.Distinct().ToList();
            if (_settings.IgnoredIds.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { _settings.IgnoredIds.Count - distinct.Count } duplicate IgnoreIds.");
                }
                else
                {
                    Log.Information($"Removing { _settings.IgnoredIds.Count - distinct.Count } duplicate IgnoreIds.");
                    _settings.IgnoredIds = distinct;
                    SaveConfig();
                }
            }
            else
            {
                if (dryRun)
                {
                    Log.Information("[DRY] No duplicate IgnoreIds found.");
                }
                else
                {
                    Log.Information("No duplicate IgnoreIds found.");
                }
            }

            distinct = _settings.TrackedSeries.Distinct().ToList();
            if (_settings.IgnoredIds.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { _settings.TrackedSeries.Count - distinct.Count } duplicate TrackedSeriesIds.");
                }
                else
                {
                    Log.Information($"Removing { _settings.TrackedSeries.Count - distinct.Count } duplicate TrackedSeriesIds.");
                    _settings.TrackedSeries = distinct;
                    SaveConfig();
                }
            }
            else
            {
                if (dryRun)
                {
                    Log.Information("[DRY] No duplicate TrackedSeriesIds found.");
                }
                else
                {
                    Log.Information("No duplicate TrackedSeriesIds found.");
                }
            }

            distinct = _settings.TrackedBooks.Distinct().ToList();
            if (_settings.TrackedBooks.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { _settings.TrackedBooks.Count - distinct.Count } duplicate TrackedBookIds.");
                }
                else
                {
                    Log.Information($"Removing { _settings.TrackedBooks.Count - distinct.Count } duplicate TrackedBookIds.");
                    _settings.TrackedBooks = distinct;
                    SaveConfig();
                }
            }
            else
            {
                if (dryRun)
                {
                    Log.Information("[DRY] No duplicate TrackedBookIds found.");
                }
                else
                {
                    Log.Information("No duplicate TrackedBookIds found.");
                }
            }
        }

        [Command(
            Name = "run",
            Description = "Run tracking.")]
        public async Task Run()
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            var dateNow = DateTime.Now;
            var seriesIds = _settings.TrackedSeries;
            var booksIds = _settings.TrackedBooks;

            Log.Debug("Removing ignored series.");
            foreach (var item in _settings.IgnoredIds)
            {
                seriesIds.Remove(item);
            }

            Log.Information($"Processing { seriesIds.Count } series.");
            var amazonSeriesList = new List<AmazonSeries>(seriesIds.Count);
            foreach (var seriesId in seriesIds)
            {
                var tempSeries = new AmazonSeries(seriesId);
                var html = await GetHtml(tempSeries.GetUri());

                try
                {
                    tempSeries.ProcessHtml(html);

                    Log.Debug("Removing ignored books from series.");
                    tempSeries.Books = tempSeries.Books.FindAll(b => !_settings.IgnoredIds.Contains(b.AmazonId));

                    amazonSeriesList.Add(tempSeries);
                }
                catch (NotSeriesException)
                {
                    Log.Error($"Skipping { seriesId.Asin }, it is not a series.");
                }
            }

            Log.Debug("Removing ignored books.");
            foreach (var item in _settings.IgnoredIds)
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
                    var html = await GetHtml(book.GetUri());
                    try
                    {
                        book.ProcessHtml(html);

                        if (_settings.IgnoreReleasedBooks)
                        {
                            if (book.ReleaseDate.AddDays((double)_settings.IgnoreAfterReleaseDays) < dateNow)
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

                var html = await GetHtml(tempBook.GetUri());
                try
                {
                    tempBook.ProcessHtml(html);

                    if (_settings.IgnoreReleasedBooks)
                    {
                        if (tempBook.ReleaseDate.AddDays((double)_settings.IgnoreAfterReleaseDays) < dateNow)
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
                    _settings.IgnoredIds.Add(item);
                }

                SaveConfig();
            }

            if (File.Exists(_pathToDataNew))
            {
                Log.Verbose("Moving old data file.");
                File.Move(_pathToDataNew, _pathToDataOld, overwrite: true);
            }

            var trackingData = new TrackingData(dateNow, amazonSeriesList, amazonBooksList);

            Log.Debug("Writing new data file.");
            using (var writer = new StreamWriter(_pathToDataNew, append: false))
            {
                string json = JsonSerializer.Serialize(trackingData, _jsonSerializerOptions);
                writer.Write(json);
            }
        }

        [Command(
            Name = "export",
            Description = "Export tracked items.")]
        public void Export(
            [Option(
                ShortName = "f",
                LongName = "outputFormat",
                Description="Output format.")]
            OutputFormat outputFormat = OutputFormat.console,
            [Option(
                LongName = "newOnly",
                Description="Export only new items.",
                BooleanMode=BooleanMode.Implicit)]
            bool newOnly = false,
            [Option(
                LongName = "append",
                Description="Append items.",
                BooleanMode=BooleanMode.Implicit)]
            bool append = false)
        {
            if (!_settingsImported)
            {
                ImportSettings();
            }

            if (!File.Exists(_pathToDataNew))
            {
                Log.Fatal("No data file found.");
                Program.Exit(ExitCode.DataFileNotFound);
            }

            var newData = ImportTrackingData(_pathToDataNew);

            var oldData = new TrackingData();
            if (File.Exists(_pathToDataOld))
            {
                oldData = ImportTrackingData(_pathToDataOld);
            }

            var analyzer = new TrackingDataAnalyzer(oldData, newData);
            Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>> data;

            if (newOnly)
            {
                data = analyzer.GetNew();
            }
            else
            {
                data = analyzer.GetAll();
            }

            // TODO: implement append
            WriteBooks(outputFormat, data.Item1, data.Item2, append);
        }

        private void WriteBooks(OutputFormat outputFormat,
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks,
            bool append)
        {
            switch (outputFormat)
            {
                case OutputFormat.console:
                    WriteBooksConsole(amazonSeries, amazonBooks);
                    break;
                case OutputFormat.csv:
                    WriteBooksCsv(amazonSeries, amazonBooks, append);
                    break;
                case OutputFormat.ics:
                    WriteBooksIcs(amazonSeries, amazonBooks, append);
                    break;
                default:
                    break;
            }
        }

        private void WriteBooksConsole(IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks)
        {
            Log.Information("Release dates:");
            foreach (var series in amazonSeries)
            {
                Log.Information($"-{ series.Title }");
                foreach (var book in series.Books)
                {
                    Log.Information($" -{ book.Title }: { book.ReleaseDate.ToString("d") }");
                }
            }

            foreach (var book in amazonBooks)
            {
                Log.Information($"-{ book.Title }: { book.ReleaseDate.ToString("d") }");
            }
        }

        private void WriteBooksCsv(IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks,
            bool append)
        {
            Log.Information("Writing csv file.");

            string fileName = $"releaseDates-{ DateTime.Now.ToString("d").Replace(" ", "") }csv";

            using (var writer = new StreamWriter(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName),
                append: false))
            using (var csv = new CsvWriter(writer, _csvConfig))
            {
                csv.Context.RegisterClassMap<AmazonBookMap>();

                csv.WriteHeader<AmazonBook>();
                csv.NextRecord();

                foreach (var series in amazonSeries)
                {
                    csv.WriteRecords(series.Books);
                }

                csv.WriteRecords(amazonBooks);
            }
        }

        private void WriteBooksIcs(IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks,
            bool append)
        {
            Log.Information("Writing ics file.");
            
            var cal = new Ical.Net.Calendar();
            cal.AddTimeZone(new VTimeZone(_settings.TimeZoneTZ));
            cal.Scale = CalendarScales.Gregorian;

            foreach (var series in amazonSeries)
            {
                foreach (var book in series.Books)
                {
                    cal.Events.Add(book.GetCalendarEvent(series.Title, _settings.IcsCategories));
                }
            }

            foreach (var book in amazonBooks)
            {
                cal.Events.Add(book.GetCalendarEvent(_settings.IcsCategories));
            }
            
            string fileName = $"releaseDates-{ DateTime.Now.ToString("d").Replace(" ", "") }ics";
            string calString = "";

            var serializer = new CalendarSerializer();
            using (var writer = new StreamWriter(@fileName, append: false))
            {
                calString = serializer.SerializeToString(cal);
                writer.Write(calString);
            }

            var ical = Ical.Net.Calendar.Load(calString);
        }

        private void ImportSettings()
        {
            if (!File.Exists(_pathToConfig))
            {
                Log.Fatal("config.json not found.");
                Program.Exit(ExitCode.ConfigNotFound);
            }

            Log.Debug("Loading config.json.");
            using (StreamReader reader = new(_pathToConfig))
            {
                try
                {
                    var settings = JsonSerializer.Deserialize<AmazonBookReleaseTrackerSettings>(reader.ReadToEnd(), _jsonSerializerOptions);

                    var settingsValidator = new AmazonBookReleaseTrackerSettingsValidator();
                    var result = settingsValidator.Validate(settings);

                    if (!result.IsValid)
                    {
                        Log.Fatal("Config not valid.");
                        foreach (var error in result.Errors)
                        {
                            Log.Fatal($"{ error.ErrorMessage }\nProperty: { error.PropertyName}, Value: { error.AttemptedValue }");
                        }

                        Program.Exit(ExitCode.ValidationError);
                    }

                    _settings = settings;
                }
                catch (Exception)
                {
                    Log.Fatal("Could not load config.json.");
                    Program.Exit(ExitCode.ConfigLoadError);
                }
            }

            _settingsImported = true;

            ConfigureHttpClient();
        }

        private TrackingData ImportTrackingData(string pathToData)
        {
            if (!File.Exists(pathToData))
            {
                Log.Fatal("Tracking data not found.");
                Program.Exit(ExitCode.DataFileNotFound);
            }

            var trackingData = new TrackingData();

            Log.Debug("Importing tracking data file.");
            using (StreamReader reader = new(pathToData))
            {
                try
                {
                    trackingData = JsonSerializer.Deserialize<TrackingData>(reader.ReadToEnd(), _jsonSerializerOptions);
                }
                catch (Exception)
                {
                    Log.Fatal("Could not load tracking data.");
                    Program.Exit(ExitCode.DataFileLoadError);
                }
            }

            return trackingData;
        }

        private void ConfigureHttpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            _client.DefaultRequestHeaders.Accept.Clear();
        }

        private void SaveConfig()
        {
            Log.Debug("Saving config.");
            using (StreamWriter writer = new StreamWriter(_pathToConfig, append: false))
            {
                string json = JsonSerializer.Serialize(
                    AmazonBookReleaseTrackerSettings.GetSettings(_settings), _jsonSerializerOptions);
                writer.Write(json);
            }
        }

        private async Task<Stream> GetHtml(Uri uri)
        {
            return await _client.GetStreamAsync(uri);
        }
    }

    public enum OutputFormat
    {
        console,
        csv,
        ics,
    }
}
