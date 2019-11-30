using homeControl.Client.WPF.ViewModels;
using homeControl.Client.WPF.ViewModels.Switches;
using Microsoft.Extensions.DependencyInjection;

namespace homeControl.Client.WPF
{
    public static class ClientWpfRegistry
    {
        public static void AddClientWpfServices(this IServiceCollection services)
        {
            services.AddTransient<AutorunConfigurator>();
            services.AddTransient<ClientViewModel>();
            services.AddTransient<SwitchViewModelsFactory>();
        }
    }
}