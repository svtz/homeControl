using System;
using System.Text;
using System.Windows;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.Interop.Rabbit.IoC;
using homeControl.SensorEmulator.ViewModels;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;
using ClientWindow = homeControl.SensorEmulator.Views.ClientWindow;

namespace homeControl.SensorEmulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger _log;
        private static readonly IConfigurationRoot _config =
            new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();

        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "sensor-emul-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder(_config)
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                    .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                    .SetupEventSender<AbstractSensorEvent>("main")
                    .SetupEventSource<AbstractSensorEvent>("main", ExchangeType.Fanout, "")
                    .Build());

                cfg.AddRegistry(new ConfigurationRegistry(serviceName));

                cfg.For<ILogger>().Use(c => _log.ForContext(c.ParentType));
            });

            return container;
        }


        private readonly IContainer _workContainer;
        public App()
        {
            _workContainer = _rootContainer.GetNestedContainer();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = new ClientWindow();
            var mainVm = _workContainer.GetInstance<ClientViewModel>();

            mainView.DataContext = mainVm;

            MainWindow = mainView;
            MainWindow.Show();
            _log.Debug("Startup complete.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _workContainer.Dispose();
            _log.Information("Exiting, return code = {ReturnCode}.", e.ApplicationExitCode);
        }

        static App()
        {
            var level = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), _config["LogEventLevel"]);

            _log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.RollingFile("logs/log-{Date}.txt", retainedFileCountLimit: 5)
                .WriteTo.Trace()
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) => _log.Fatal("Необработанное исключение: {Exception}", e.ExceptionObject);

            _log.Debug("Logging initialized.");
        }
    }
}
