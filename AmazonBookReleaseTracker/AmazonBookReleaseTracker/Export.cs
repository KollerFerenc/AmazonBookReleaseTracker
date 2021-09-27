using CommandDotNet;
using CsvHelper;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    [Command(
        Name = "export",
        Description = "Export tracked items.")]
    public class Export
    {
        public Config Config { get; private set; } = new();

        [Command(
            Name = "console",
            Description = "Export tracked items to the console.")]
        public int ExportConsole([Option(
                LongName = "newOnly",
                Description = "Export only new items.",
                BooleanMode = BooleanMode.Implicit)]
            bool newOnly)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var data = GetData(newOnly);

            Console.WriteLine("Release dates:");
            foreach (var series in data.AmazonSeries)
            {
                Console.WriteLine($"-{ series.Title }");
                foreach (var book in series.Books)
                {
                    Console.WriteLine($" -{ book.Title }: { book.ReleaseDate.ToString("d") }");
                }
            }

            foreach (var book in data.AmazonBooks)
            {
                Console.WriteLine($"-{ book.Title }: { book.ReleaseDate.ToString("d") }");
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "csv",
            Description = "Export tracked items to CSV file.")]
        public int ExportCsv([Option(
                LongName = "newOnly",
                Description = "Export only new items.",
                BooleanMode = BooleanMode.Implicit)]
            bool newOnly,
            [Option(
                LongName = "append",
                Description="Append items.",
                BooleanMode=BooleanMode.Implicit)]
            bool append)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var data = GetData(newOnly);

            Log.Information("Writing csv file.");

            using (var writer = new StreamWriter(
                Config.Settings.GetExportPath("csv"),
                append: append))
            using (var csv = new CsvWriter(writer, Utilities.csvConfig))
            {
                csv.Context.RegisterClassMap<AmazonBookMap>();

                if (!append)
                {
                    csv.WriteHeader<AmazonBook>();
                }
                csv.NextRecord();

                foreach (var series in data.AmazonSeries)
                {
                    csv.WriteRecords(series.Books);
                }

                csv.WriteRecords(data.AmazonBooks);
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "calendar",
            Description = "Export tracked items to calendar file.")]
        public int ExportCalendar([Option(
                LongName = "newOnly",
                Description = "Export only new items.",
                BooleanMode = BooleanMode.Implicit)]
            bool newOnly,
            [Option(
                LongName = "append",
                Description="Append items.",
                BooleanMode=BooleanMode.Implicit)]
            bool append)
        {
            if (!Config.SettingsImported)
            {
                var result = Config.ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var data = GetData(newOnly);

            Log.Information("Writing iCalendar file.");

            var cal = new Ical.Net.Calendar();
            string calString = "";

            if (append)
            {
                calString = File.ReadAllText(Config.Settings.GetExportPath("ics"));
                cal = Ical.Net.Calendar.Load(calString);
            }
            else
            {
                cal.AddTimeZone(new VTimeZone(Config.Settings.TimeZoneTZ));
                cal.Scale = CalendarScales.Gregorian;
            }

            foreach (var series in data.AmazonSeries)
            {
                cal.Events.Add(series, Config.Settings.IcsCategories);
            }

            foreach (var book in data.AmazonBooks)
            {
                cal.Events.Add(book, Config.Settings.IcsCategories);
            }

            calString = "";
            var serializer = new CalendarSerializer();

            using (var writer = new StreamWriter(
                Config.Settings.GetExportPath("ics"),
                append: false))
            {
                calString = serializer.SerializeToString(cal);
                writer.Write(calString);
            }

            return (int)ExitCode.Default;
        }

        public AmazonContainer GetData(bool newOnly)
        {
            if (!Config.SettingsImported)
            {
                Config.ImportSettings();
            }

            if (!File.Exists(Utilities.pathToDataNew))
            {
                Log.Fatal("No data file found.");
                return new AmazonContainer();
            }

            var newData = ImportTrackingData(Utilities.pathToDataNew);

            var oldData = new TrackingData();
            if (File.Exists(Utilities.pathToDataOld))
            {
                oldData = ImportTrackingData(Utilities.pathToDataOld);
            }

            var analyzer = new TrackingDataAnalyzer(oldData, newData);
            AmazonContainer data;

            if (newOnly)
            {
                data = analyzer.GetNew();
            }
            else
            {
                data = analyzer.GetAll();
            }

            return data;
        }

        private static TrackingData ImportTrackingData(string pathToData)
        {
            if (!File.Exists(pathToData))
            {
                Log.Fatal("Tracking data not found.");
                return new TrackingData();
            }

            var trackingData = new TrackingData();

            Log.Debug("Importing tracking data file.");
            using (StreamReader reader = new(pathToData))
            {
                try
                {
                    trackingData = JsonSerializer.Deserialize<TrackingData>(reader.ReadToEnd(), Utilities.jsonSerializerOptions);
                }
                catch (Exception)
                {
                    Log.Fatal("Could not load tracking data.");
                    return new TrackingData();
                }
            }

            return trackingData;
        }
    }
}
