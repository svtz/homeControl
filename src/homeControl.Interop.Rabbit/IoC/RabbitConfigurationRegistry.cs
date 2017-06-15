using System;
using System.Collections.Generic;
using StructureMap;

namespace homeControl.Interop.Rabbit.IoC
{
    internal sealed class RabbitConfigurationRegistry : Registry
    {
        public RabbitConfigurationRegistry(IEnumerable<Action<Registry>> configActions)
        {
            Guard.DebugAssertArgumentNotNull(configActions, nameof(configActions));

            foreach (var configAction in configActions)
            {
                configAction(this);
            }
        }
    }
}
