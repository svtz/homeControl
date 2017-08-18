using System;
using System.Text;
using System.Windows;
using homeControl.Client.WPF.ViewModels;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Switches;
using homeControl.Interop.Rabbit.IoC;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using StructureMap;
using ClientWindow = homeControl.Client.WPF.Views.ClientWindow;

namespace homeControl.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger _log = CreateLogger();

        private static readonly IContainer _rootContainer = BuildContainer();
        private static IContainer BuildContainer()
        {
            var serviceName = "wpf-" + Guid.NewGuid();

            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new RabbitConfigurationRegistryBuilder("amqp://client:client@192.168.1.17/debug")
                    .UseJsonSerializationWithEncoding(Encoding.UTF8)
                    .SetupEventSender<ConfigurationRequestEvent>("configuration_requests", ExchangeType.Fanout)
                    .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                    .SetupEventSender<AbstractBindingEvent>("main", ExchangeType.Fanout)
                    .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
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
            _workContainer.GetInstance<AutorunConfigurator>().SetupAutoRun();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = new ClientWindow();
            var mainVm = _workContainer.GetInstance<ClientViewModel>();

            mainView.DataContext = mainVm;

            MainWindow = mainView;

            _log.Debug("Startup complete.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _workContainer.Dispose();
            _log.Information("Exiting, return code = {ReturnCode}.", e.ApplicationExitCode);
        }

        private static ILogger CreateLogger()
        {
#if DEBUG
            var level = LogEventLevel.Verbose;
#else
            var level = LogEventLevel.Information;
#endif

            var log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.RollingFile("logs/log-{Date}.txt", retainedFileCountLimit: 5)
                .WriteTo.Trace()
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) => log.Fatal("Необработанное исключение: {Exception}", e.ExceptionObject);

            log.Debug("Logging initialized.");

            return log;
        }
    }
}
