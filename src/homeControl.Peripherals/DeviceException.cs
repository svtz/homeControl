using System;

namespace homeControl.Peripherals
{
    public abstract class DeviceException : Exception
    {
        protected DeviceException(string message) : base(message)
        {
        }

        protected DeviceException(Exception innerException, string message) : base(message, innerException)
        {
        }
    }
}