using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
                    typeof(WebApiEntryPoint),
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

                if (plugin.Instances.Count() > 1)
                {
                    var arrayType = pluginType.MakeArrayType();
                    var arrayDescription = ServiceDescriptor.Transient(arrayType, s => GetAllInstancesAsArray(pluginType));
                    services.Add(arrayDescription);
                }

                if (plugin.Default != null)
                {
                    var serviceDescription = ServiceDescriptor.Transient(pluginType, s => _container.GetInstance(pluginType));
                    services.Add(serviceDescription);
                }
            }
        }

        private Array GetAllInstancesAsArray(Type pluginType)
        {
            var param = Expression.Parameter(typeof(IEnumerable), "instance");
            var cast = Expression.Call(typeof(Enumerable), nameof(Enumerable.Cast), new[] { pluginType }, param);
            var toArray = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { pluginType }, cast);
            var lambda = Expression.Lambda<Func<IEnumerable, Array>>(toArray, param).Compile();

            return lambda(_container.GetAllInstances(pluginType));
        }
    }
}
