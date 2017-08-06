namespace homeControl.NooliteService.Diagnostics
{
    public sealed class DeviceInitializationException : DeviceException
    {
        public DeviceInitializationException(string deviceName, string description)
            : base($@"Device initialization error: {deviceName}.
{description}")
        {
        }
    }
}