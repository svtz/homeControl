using Serilog;
using Serilog.Events;

namespace homeControl.Tests
{
    internal static class TestLoggerHolder
    {
        public static ILogger Logger { get; }

        static TestLoggerHolder()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Trace(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (from {SourceContext}){NewLine}{Exception}")
                .CreateLogger();
            
            Logger.Debug("Test logging initialized.");
        }
    }
}