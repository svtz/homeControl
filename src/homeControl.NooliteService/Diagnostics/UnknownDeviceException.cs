using System;

namespace homeControl.NooliteService.Diagnostics
{
    public sealed class UnknownDeviceException : DeviceException
    {
        public UnknownDeviceException(Exception innerException, string deviceName)
            : base(innerException, $"Unknown error while using device:  {deviceName}.")
        {
        }
    }
}