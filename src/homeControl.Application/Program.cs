using System;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Application.IoC;
using homeControl.Configuration;
using homeControl.Core;
using homeControl.WebApi;
using StructureMap;

namespace homeControl.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new Container(c => c.AddRegistry<ApplicationRegistry>());
            using (var child = container.GetNestedContainer())
            {
                Run(child);
            }
        }

        private static void Run(IContainer child)
        {
            var initializers = child.GetAllInstances<IInitializer>();
            foreach (var initializer in initializers)
            {
                initializer.Init();
            }

            var loop = child.GetInstance<EventProcessingLoop>();
            loop.ThrottleTime = TimeSpan.FromMilliseconds(100);

            var entryPoint = child.GetInstance<WebApiEntryPoint>();

            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => cts.Cancel();
                var loopTask = Task.Factory.StartNew(() => loop.Run(cts.Token));
                var apiTask = Task.Factory.StartNew(() => entryPoint.Run(cts.Token));

                Task.WaitAll(loopTask, apiTask);
            }
        }
    }
}
