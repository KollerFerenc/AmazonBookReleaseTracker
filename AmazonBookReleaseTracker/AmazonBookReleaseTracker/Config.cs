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
            SaveConfig(Utilities.pathToConfig);
            return (int)ExitCode.Default;
        }

        [DefaultMethod, Command(
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
            Name = "backup",
            Description = "Backup config.")]
        public int Backup()
        {
            if (!SettingsImported)
            {
                var result = ImportSettings();
                if (result != ExitCode.Default)
                {
                    return (int)result;
                }
            }

            try
            {
                SaveConfig(Path.Combine(Program.baseDirectory, $@"config-backup-{ DateTime.Now.ToShortDateString().Replace(" ", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty) }.json"));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error while saving backup.");
                return (int)ExitCode.BackupSaveError;
            }

            return (int)ExitCode.Default;
        }

        internal void SaveConfig(string pathToFile)
        {
            Log.Debug("Saving config.");
            using (StreamWriter writer = new(pathToFile, append: false))
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
