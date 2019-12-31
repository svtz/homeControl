using System;
using System.Text;
using System.Windows;
using homeControl.Client.WPF.ViewModels;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Switches;
using homeControl.Interop.Rabbit.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using ClientWindow = homeControl.Client.WPF.Views.ClientWindow;

namespace homeControl.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger Log;
        private static readonly IConfigurationRoot Config =
            new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();

        private static readonly IServiceProvider RootServiceProvider;
        private static IServiceProvider BuildServiceProvider()
        {
            var serviceName = "wpf-" + Guid.NewGuid();

            var services = new ServiceCollection();
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventSource<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                .SetupEventSender<AbstractSwitchEvent>("main")
                .SetupEventSender<AbstractBindingEvent>("main")
                .SetupEventSender<NeedStatusEvent>("main")
                .SetupEventSource<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .SetupEventSource<AbstractBindingEvent>("main", ExchangeType.Fanout, "")
                .Apply(services);

            services.AddConfigurationRepositories(serviceName);
            services.AddSingleton<ILogger>(Log);
            services.AddClientWpfServices();
            
            return services.BuildServiceProvider();
        }


        private readonly IServiceScope _serviceScope;
        public App()
        {
            _serviceScope = RootServiceProvider.CreateScope();
            _serviceScope.ServiceProvider.GetRequiredService<AutorunConfigurator>().SetupAutoRun();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainView = new ClientWindow();
            var mainVm = _serviceScope.ServiceProvider.GetRequiredService<ClientViewModel>();

            mainView.DataContext = mainVm;

            MainWindow = mainView;

            Log.Debug("Startup complete.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _serviceScope.Dispose();
            Log.Information("Exiting, return code = {ReturnCode}.", e.ApplicationExitCode);
        }

        static App()
        {
            var level = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), Config["LogEventLevel"]);

            Log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.File("logs/log-{Date}.txt", retainedFileCountLimit: 5)
                .WriteTo.Trace()
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Fatal("Необработанное исключение: {Exception}", e.ExceptionObject);

            Log.Debug("Logging initialized.");

            RootServiceProvider = BuildServiceProvider();
        }
    }
}
