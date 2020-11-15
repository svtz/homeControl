using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace homeControl.Entry
{
    public class LoggerBuilder
    {
        private readonly IConfigurationRoot _config;

        public LoggerBuilder(IConfigurationRoot config)
        {
            Guard.DebugAssertArgumentNotNull(config, nameof(config));
            _config = config;
        }
        
        public ILogger BuildLogger()
        {
            var level = (LogEventLevel) Enum.Parse(typeof(LogEventLevel), _config["LogEventLevel"]);

            var context = Assembly.GetEntryAssembly().EntryPoint.ReflectedType;
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (from {SourceContext}){NewLine}{Exception}")
                .WriteTo.File("logs/log.txt", retainedFileCountLimit: 30, 
                    rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger()
                .ForContext(context);

            Log.Logger = logger;

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += LogFirstChanceException;
            
            logger.Debug("Logging initialized.");
            return logger;
        }

        private void LogFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Log.Logger.Verbose(e.Exception, "First chance exception: {message}", e.Exception.Message);
        }

        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception!");
        }
    }
}