using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using homeControl.ConfigurationStore.IoC;
using homeControl.Domain.Events.Configuration;
using homeControl.Entry;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Serilog;

namespace homeControl.ConfigurationStore
{
    internal sealed class ConfigurationStoreService : AbstractService
    {
        protected override string ServiceName => "ConfigStore";
        
        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken ct)
        {
            await serviceProvider.GetRequiredService<ConfigurationRequestsProcessor>().Run(ct);
        }

        protected override void ConfigureServices(ServiceCollection services)
        {
            services.AddConfigurationStoreServices();
        }

        private static async Task Main(string[] args)
        {
            await new ConfigurationStoreService().Run();
        }
    }
}