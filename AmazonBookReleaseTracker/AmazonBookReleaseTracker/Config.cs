using CommandDotNet;
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
        Name = "config",
        Description = "Configuration.")]
    public class Config
    {
        public bool SettingsImported { get; private set; }
        public IAmazonBookReleaseTrackerSettings Settings { get; private set; }

        [Command(
            Name = "init",
            Description = "Initialize config.")]
        public int Init(
            [Option(
                LongName = "force",
                ShortName = "f",
                Description="Overwrite existing configuration.",
                BooleanMode=BooleanMode.Implicit)]
            bool force)
        {
            Log.Information("Initialize config.");

            if (File.Exists(Utilities.pathToConfig))
            {
                if (!force)
                {
                    Log.Fatal("Config already exists.");
                    return (int)ExitCode.ConfigExists;
                }
            }

            if (File.Exists(Utilities.pathToDataOld))
            {
                if (force)
                {
                    File.Delete(Utilities.pathToDataOld);
                }
            }

            if (File.Exists(Utilities.pathToDataNew))
            {
                if (force)
                {
                    File.Delete(Utilities.pathToDataNew);
                }
            }

            Settings = new AmazonBookReleaseTrackerSettings();

            SettingsImported = true;
            SaveConfig();
            return (int)ExitCode.Default;
        }

        [Command(
            Name = "show",
            Description = "Show current config.")]
        public int ShowConfig(
            [Option(
                LongName = "path",
                Description="Show config path.",
                BooleanMode=BooleanMode.Implicit)]
            bool path)
        {
            if (!SettingsImported)
            {
                var result = ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            if (path)
            {
                Log.Information(Utilities.pathToConfig);
            }
            else
            {
                Log.Debug("Writing config to console.");
                Console.WriteLine(JsonSerializer.Serialize(
                    AmazonBookReleaseTrackerSettings.GetSettings(Settings), Utilities.jsonSerializerOptions));
            }

            return (int)ExitCode.Default;
        }

        [Command(
            Name = "removeDuplicates",
            Description = "Remove duplicate ASINs from config.")]
        public int RemoveDuplicates(
            [Option(
                LongName = "dryRun",
                Description= "Dry run.",
                BooleanMode=BooleanMode.Implicit)]
            bool dryRun)
        {
            if (!SettingsImported)
            {
                var result = ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            var distinct = Settings.IgnoredIds.Distinct().ToList();
            if (Settings.IgnoredIds.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { Settings.IgnoredIds.Count - distinct.Count } duplicate IgnoreIds.");
                }
                else
                {
                    Log.Information($"Removing { Settings.IgnoredIds.Count - distinct.Count } duplicate IgnoreIds.");
                    Settings.IgnoredIds = distinct;
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

            distinct = Settings.TrackedSeries.Distinct().ToList();
            if (Settings.IgnoredIds.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { Settings.TrackedSeries.Count - distinct.Count } duplicate TrackedSeriesIds.");
                }
                else
                {
                    Log.Information($"Removing { Settings.TrackedSeries.Count - distinct.Count } duplicate TrackedSeriesIds.");
                    Settings.TrackedSeries = distinct;
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

            distinct = Settings.TrackedBooks.Distinct().ToList();
            if (Settings.TrackedBooks.Count - distinct.Count > 0)
            {
                if (dryRun)
                {
                    Log.Information($"[DRY] Removing { Settings.TrackedBooks.Count - distinct.Count } duplicate TrackedBookIds.");
                }
                else
                {
                    Log.Information($"Removing { Settings.TrackedBooks.Count - distinct.Count } duplicate TrackedBookIds.");
                    Settings.TrackedBooks = distinct;
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

            return (int)ExitCode.Default;
        }

        internal void SaveConfig()
        {
            Log.Debug("Saving config.");
            using (StreamWriter writer = new StreamWriter(Utilities.pathToConfig, append: false))
            {
                string json = JsonSerializer.Serialize(
                    AmazonBookReleaseTrackerSettings.GetSettings(Settings), Utilities.jsonSerializerOptions);
                writer.Write(json);
            }
        }

        internal ExitCode ImportSettings()
        {
            if (!File.Exists(Utilities.pathToConfig))
            {
                Log.Fatal("config.json not found.");
                return ExitCode.ConfigNotFound;
            }

            Log.Debug("Loading config.json.");
            using (StreamReader reader = new(Utilities.pathToConfig))
            {
                try
                {
                    var settings = JsonSerializer.Deserialize<AmazonBookReleaseTrackerSettings>(reader.ReadToEnd(), Utilities.jsonSerializerOptions);

                    var settingsValidator = new AmazonBookReleaseTrackerSettingsValidator();
                    var result = settingsValidator.Validate(settings);

                    if (!result.IsValid)
                    {
                        Log.Fatal("Config not valid.");
                        foreach (var error in result.Errors)
                        {
                            Log.Fatal($"{ error.ErrorMessage }\nProperty: { error.PropertyName}, Value: { error.AttemptedValue }");
                        }

                        return ExitCode.ValidationError;
                    }

                    Settings = settings;
                }
                catch (Exception)
                {
                    Log.Fatal("Could not load config.json.");
                    return ExitCode.ConfigLoadError;
                }
            }

            SettingsImported = true;
            return ExitCode.Default;
        }
    }
}
