using System;

namespace homeControl.Peripherals
{
    public sealed class UnknownDeviceException : DeviceException
    {
        public UnknownDeviceException(Exception innerException, string deviceName)
            : base(innerException, $"Unknown error while using device:  {deviceName}.")
        {
        }
    }
}