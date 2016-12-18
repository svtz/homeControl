using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using homeControl.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using StructureMap.TypeRules;

namespace homeControl.WebApi
{
    public sealed class WebApiEntryPoint
    {
        private readonly IContainer _container;

        public WebApiEntryPoint(IContainer container)
        {
            _container = container;
        }

        public void Run(CancellationToken ct)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(ForwardContainerRegistrations)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build();

            host.Run(ct);
        }

        private Assembly[] GetForwardTypeAssemblies()
        {
            return new[]
                {
                    typeof(IEventPublisher),
                }
                .Select(type => type.GetAssembly())
                .ToArray();
        }

        private void ForwardContainerRegistrations(IServiceCollection services)
        {
            var forwardAssemblies = GetForwardTypeAssemblies();
            foreach (var plugin in _container.Model.PluginTypes)
            {
                var pluginType = plugin.PluginType;
                var pluginAssembly = pluginType.GetAssembly();
                if (!forwardAssemblies.Contains(pluginAssembly))
                {
                    continue;
                }

                var serviceDescription = ServiceDescriptor.Transient(pluginType, s => _container.GetInstance(pluginType));
                services.Add(serviceDescription);
            }
        }
    }
}
