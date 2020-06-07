using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace homeControl.Entry
{
    public abstract class AbstractService
    {
        protected IConfigurationRoot Config { get; }
        protected ILogger Logger { get; }

        protected AbstractService()
        {
            Config = new ConfigReader().ReadConfig();
            Logger = new LoggerBuilder(Config).BuildLogger();
        }
        
        protected abstract string ServiceNamePrefix { get; }
        protected abstract Task Run(IServiceProvider serviceProvider, CancellationToken ct);

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


        protected async Task Run()
        {
            var version = GetType().Assembly.GetName().Version;
            var title = $"{ServiceNamePrefix} v.{version.ToString(3)}";
            Console.Title = title;

            Logger.Information($"Starting service: {title}");

            using var serviceScope = CreateRootServiceProvider().CreateScope();
            var cts = serviceScope.ServiceProvider.GetRequiredService<CancellationTokenSource>();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            await Run(serviceScope.ServiceProvider, cts.Token);
        }
    }
}