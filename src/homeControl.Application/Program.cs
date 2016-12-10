using System;
using homeControl.Application.IoC;
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
                container.AssertConfigurationIsValid();
                //var loop = child.GetInstance<EventProcessingLoop>();

            }
        }
    }
}
