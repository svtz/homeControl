using JetBrains.Annotations;

namespace homeControl.Configuration
{
    [UsedImplicitly]
    internal sealed class ServiceNameHolder
    {
        public string ServiceName { get; }
        
        public ServiceNameHolder(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}