using System;
using System.Text;
using System.Threading;
using System.Windows;
using homeControl.Client.WPF.ViewModels;
using homeControl.Configuration.IoC;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using homeControl.Domain.Events.Configuration;
using homeControl.Domain.Events.Switches;
using homeControl.Entry;
using homeControl.Interop.Rabbit.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using ClientWindow = homeControl.Client.WPF.Views.ClientWindow;

namespace homeControl.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger Log { get; }
        private IConfigurationRoot Config { get; }

        private IServiceProvider BuildServiceProvider()
        {
            var serviceName = "wpf-" + Guid.NewGuid();

            var services = new ServiceCollection();
            new RabbitConfiguration(Config)
                .UseJsonSerializationWithEncoding(Encoding.UTF8)
                .SetupEventSender<ConfigurationRequestEvent>("configuration-requests")
                .SetupEventReceiver<ConfigurationResponseEvent>("configuration", ExchangeType.Direct, serviceName)
                .SetupEventSender<AbstractSwitchEvent>("main")
                .SetupEventSender<AbstractBindingEvent>("main")
                .SetupEventSender<NeedStatusEvent>("main")
                .SetupEventReceiver<AbstractSwitchEvent>("main", ExchangeType.Fanout, "")
                .SetupEventReceiver<AbstractBindingEvent>("main", ExchangeType.Fanout, "")
                .Apply(services);

            services.AddConfigurationRepositories(serviceName);
            services.AddSingleton<ILogger>(Log);
            services.AddClientWpfServices();
            services.AddSingleton<CancellationTokenSource>();
            
            return services.BuildServiceProvider();
        }


        private readonly IServiceScope _serviceScope;
        public App()
        {            
            Config = new ConfigReader().ReadConfig();
            Log = new LoggerBuilder(Config).BuildLogger();
            _serviceScope = BuildServiceProvider().CreateScope();
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
    }
}
