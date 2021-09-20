﻿using CommandDotNet;
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
        public void ExportConsole([Option(
                LongName = "newOnly",
                Description = "Export only new items.",
                BooleanMode = BooleanMode.Implicit)]
            bool newOnly)
        {
            if (!Config.SettingsImported)
            {
                Config.ImportSettings();
            }

            var data = GetData(newOnly);

            Log.Information("Release dates:");
            foreach (var series in data.Item1)
            {
                Log.Information($"-{ series.Title }");
                foreach (var book in series.Books)
                {
                    Log.Information($" -{ book.Title }: { book.ReleaseDate.ToString("d") }");
                }
            }

            foreach (var book in data.Item2)
            {
                Log.Information($"-{ book.Title }: { book.ReleaseDate.ToString("d") }");
            }
        }

        [Command(
            Name = "csv",
            Description = "Export tracked items to CSV file.")]
        public void ExportCsv([Option(
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
                Config.ImportSettings();
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

                foreach (var series in data.Item1)
                {
                    csv.WriteRecords(series.Books);
                }

                csv.WriteRecords(data.Item2);
            }
        }

        [Command(
            Name = "calendar",
            Description = "Export tracked items to calendar file.")]
        public void ExportCalendar([Option(
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
                Config.ImportSettings();
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

            foreach (var series in data.Item1)
            {
                foreach (var book in series.Books)
                {
                    int index = cal.Events.GetIndexOfUid(book.GetGuid().ToString("D"));

                    if (index != -1)
                    {
                        cal.Events.Remove(cal.Events[index]);
                    }

                    cal.Events.Add(book.GetCalendarEvent(series.Title, Config.Settings.IcsCategories));
                }
            }

            foreach (var book in data.Item2)
            {
                int index = cal.Events.GetIndexOfUid(book.GetGuid().ToString("D"));

                if (index != -1)
                {
                    cal.Events.Remove(cal.Events[index]);
                }

                cal.Events.Add(book.GetCalendarEvent(book.Title, Config.Settings.IcsCategories));
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
        }

        private Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>> GetData(bool newOnly)
        {
            if (!File.Exists(Utilities.pathToDataNew))
            {
                Log.Fatal("No data file found.");
                Program.Exit(ExitCode.DataFileNotFound);
            }

            var newData = ImportTrackingData(Utilities.pathToDataNew);

            var oldData = new TrackingData();
            if (File.Exists(Utilities.pathToDataOld))
            {
                oldData = ImportTrackingData(Utilities.pathToDataOld);
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

            return data;
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
                    trackingData = JsonSerializer.Deserialize<TrackingData>(reader.ReadToEnd(), Utilities.jsonSerializerOptions);
                }
                catch (Exception)
                {
                    Log.Fatal("Could not load tracking data.");
                    Program.Exit(ExitCode.DataFileLoadError);
                }
            }

            return trackingData;
        }
    }
}