using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace homeControl.Entry
{
    public abstract class AbstractService
    {
        protected static IConfigurationRoot Config => ConfigHolder.Config;
        protected static ILogger Logger => LoggerHolder.Logger;
        
        protected abstract string ServiceNamePrefix { get; }
        protected abstract void Run(IServiceProvider serviceProvider, CancellationToken ct);

        protected abstract void ConfigureServices(ServiceCollection services, string uniqueServiceName);
        
        private IServiceProvider CreateRootServiceProvider()
        {
            var uniqueServiceName = $"{ServiceNamePrefix}-{Guid.NewGuid()}";

            var services = new ServiceCollection();
            services.AddSingleton(sp => Logger);
            services.AddSingleton(new CancellationTokenSource());
            ConfigureServices(services, uniqueServiceName);

            Logger.Debug($"Root service provider created");
            
            return services.BuildServiceProvider();
        }


        protected void Run()
        {
            var version = GetType().Assembly.GetName().Version;
            var title = $"{ServiceNamePrefix} v.{version.ToString(3)}";
            Console.Title = title;

            Logger.Information($"Starting service: {title}");

            using var serviceScope = CreateRootServiceProvider().CreateScope();
            var cts = serviceScope.ServiceProvider.GetRequiredService<CancellationTokenSource>();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            Run(serviceScope.ServiceProvider, cts.Token);
        }
    }
}