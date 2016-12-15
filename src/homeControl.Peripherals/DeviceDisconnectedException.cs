namespace homeControl.Peripherals
{
    public sealed class DeviceDisconnectedException : DeviceException
    {
        public DeviceDisconnectedException(string deviceName)
            : base($"Device is not connected: {deviceName}.")
        {
        }
    }
}