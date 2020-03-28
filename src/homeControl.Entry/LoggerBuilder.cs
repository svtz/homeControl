using System;
using System.Collections.Concurrent;
using System.Reflection;
using Serilog;
using Serilog.Events;

namespace homeControl.Entry
{
    internal static class LoggerHolder
    {
        public static ILogger Logger { get; }

        static LoggerHolder()
        {
            var level = (LogEventLevel) Enum.Parse(typeof(LogEventLevel), ConfigHolder.Config["LogEventLevel"]);

            var context = Assembly.GetEntryAssembly().EntryPoint.ReflectedType;
            Logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (from {SourceContext}){NewLine}{Exception}")
                .WriteTo.File("logs/log-{Date}.txt", retainedFileCountLimit: 5, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024)
                .CreateLogger()
                .ForContext(context);
            
            Logger.Debug("Logging initialized.");
        }
    }
}