using System;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Application.IoC;
using homeControl.Configuration;
using homeControl.Core;
using StructureMap;

namespace homeControl.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new Container(c => c.AddRegistry<ApplicationRegistry>());
            using (var child = container.GetNestedContainer())
            using (var cts = new CancellationTokenSource())
            {
                child.Configure(c => c.For<CancellationToken>().Use(() => cts.Token));

                Console.CancelKeyPress += (s, e) => cts.Cancel();

                Run(child, cts.Token);
            }
        }

        private static void Run(IContainer child, CancellationToken ct)
        {
            var initializers = child.GetAllInstances<IInitializer>();
            foreach (var initializer in initializers)
            {
                initializer.Init();
            }

            var loop = child.GetInstance<EventProcessingLoop>();
            loop.ThrottleTime = TimeSpan.FromMilliseconds(100);

            var loopTask = Task.Factory.StartNew(() => loop.Run(ct), ct);
            Task.WaitAll(loopTask);
        }
    }
}
