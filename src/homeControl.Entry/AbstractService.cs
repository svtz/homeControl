using System;
using System.Collections.Generic;
using System.Threading;
using homeControl.Configuration.IoC;
using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace homeControl.Entry
{
    public abstract class AbstractService
    {
        protected static IConfigurationRoot Config => ConfigHolder.Config;
        protected static ILogger Logger => LoggerHolder.Logger;
        
        protected abstract string ServiceNamePrefix { get; }
        protected abstract IEnumerable<Registry> GetConfigurationRegistries(string uniqueServiceName);
        protected abstract void Run(IContainer container, CancellationToken ct);

        private IContainer CreateRootContainer()
        {
            var uniqueServiceName = $"{ServiceNamePrefix}-{Guid.NewGuid()}";

            var container = new Container(cfg =>
            {
                cfg.For<ILogger>().Use(c => Logger.ForContext(c.ParentType));

                foreach (var registry in GetConfigurationRegistries(uniqueServiceName))
                {
                    cfg.AddRegistry(registry);
                }
            });

            return container;
        }
        
        protected void Run()
        {
            var version = GetType().Assembly.GetName().Version;
            var title = $"{ServiceNamePrefix} v.{version.ToString(3)}";
            Console.Title = title;

            Logger.Information($"Starting service: {title}");

            using (var workContainer = CreateRootContainer().GetNestedContainer())
            using (var cts = new CancellationTokenSource())
            {
                workContainer.Configure(c => c.For<CancellationToken>().Use(() => cts.Token));

                Console.CancelKeyPress += (s, e) => cts.Cancel();

                Run(workContainer, cts.Token);
            }
        }
    }
}