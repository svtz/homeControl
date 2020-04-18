using System;
using System.Threading.Tasks;
using System.Windows;
using homeControl.Client.WPF.ViewModels;
using homeControl.Configuration.IoC;
using homeControl.Entry;
using homeControl.Interop.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
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

        private const string ServiceName = "WPF";
        
        private async Task<(IServiceProvider, IEndpointInstance)> BuildServiceProviderAndStartNsb()
        {
            var services = new ServiceCollection();

            services.AddConfigurationRepositories(ServiceName);
            services.AddSingleton<ILogger>(Log);
            services.AddClientWpfServices();

            var endpoint = await new EndpointBuilder(Config, Log)
                .UseEndpointName(ServiceName)
                .Build(services);
            
            return (services.BuildServiceProvider(), endpoint);
        }


        private IServiceScope _serviceScope;
        private IEndpointInstance _endpoint;
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var (serviceProvider, endpoint) = await BuildServiceProviderAndStartNsb();
            _endpoint = endpoint;
            
            _serviceScope = serviceProvider.CreateScope();
            _serviceScope.ServiceProvider.GetRequiredService<AutorunConfigurator>().SetupAutoRun();
            
            var mainView = new ClientWindow();
            var mainVm = _serviceScope.ServiceProvider.GetRequiredService<ClientViewModel>();

            mainView.DataContext = mainVm;

            MainWindow = mainView;

            Log.Debug("Startup complete.");
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            await (_endpoint?.Stop() ?? Task.CompletedTask);
            _serviceScope?.Dispose();
            Log.Information("Exiting, return code = {ReturnCode}.", e.ApplicationExitCode);
        }

        public App()
        {
            Config = new ConfigReader().ReadConfig();
            Log = new LoggerBuilder(Config).BuildLogger();
        }
    }
}
