using System;
using System.Threading;
using homeControl.Application.IoC;
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
            {
                //container.AssertConfigurationIsValid();
                var loop = child.GetInstance<EventProcessingLoop>();
                loop.ThrottleTime = TimeSpan.FromMilliseconds(100);

                using (var cts = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) => cts.Cancel();
                    loop.Run(cts.Token);
                }
            }
        }
    }
}
