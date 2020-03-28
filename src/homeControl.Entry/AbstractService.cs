using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Interop.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Serilog;

namespace homeControl.Entry
{
    public abstract class AbstractService
    {
        protected static IConfigurationRoot Config => ConfigHolder.Config;
        protected static ILogger Logger => LoggerHolder.Logger;
        
        protected abstract string ServiceName { get; }
        protected abstract Task Run(IServiceProvider serviceProvider, CancellationToken ct);

        protected abstract void ConfigureServices(ServiceCollection services);

        private async Task<(IServiceProvider, IEndpointInstance)> CreateRootServiceProviderAndStartEndpoint()
        {
            var services = new ServiceCollection();
            services.AddSingleton(sp => Logger);
            services.AddSingleton(new CancellationTokenSource());
            ConfigureServices(services);

            Logger.Debug($"Root service provider created");

            var endpoint = await new EndpointBuilder(Config, Logger)
                .UseEndpointName(ServiceName)
                .Build(services);
            return (services.BuildServiceProvider(), endpoint);
        }


        protected async Task Run()
        {
            var version = GetType().Assembly.GetName().Version;
            var title = $"{ServiceName} v.{version.ToString(3)}";
            Console.Title = title;

            Logger.Information($"Starting service: {title}");

            var (rootProvider, endpoint) = await CreateRootServiceProviderAndStartEndpoint();
            
            using var serviceScope = rootProvider.CreateScope();
            var cts = serviceScope.ServiceProvider.GetRequiredService<CancellationTokenSource>();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            await Run(serviceScope.ServiceProvider, cts.Token);
            await endpoint.Stop();
        }
    }
}