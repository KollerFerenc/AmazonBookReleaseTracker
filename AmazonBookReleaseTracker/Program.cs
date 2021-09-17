using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using Serilog;
using CommandDotNet;
using CommandDotNet.FluentValidation;

namespace AmazonBookReleaseTracker
{
    class Program
    {
        internal static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args)
        {
            ConfigureLogger();
            
            var appRunner = new AppRunner<AmazonBookReleaseTracker>()
                .UseDefaultMiddleware()
                .UseFluentValidation(showHelpOnError: true);

            appRunner.Run(args);

            Exit(ExitCode.Default);
        }

        private static void ConfigureLogger()
        {
            var logLevel = Serilog.Events.LogEventLevel.Information;

            #if DEBUG
            logLevel = Serilog.Events.LogEventLevel.Debug;
            #endif

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .WriteTo.Console()
                .WriteTo.File(Path.Join(baseDirectory, @"logs\log-.txt"),
                    rollingInterval: RollingInterval.Year,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        internal static void Exit(ExitCode exitCode)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)exitCode);
        }
    }
}
