using System;

namespace homeControl.Configuration
{
    public sealed class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message)
        {
        }

        public InvalidConfigurationException(Exception innerException, string message) : base(message, innerException)
        {
        }
    }
}