using System;

namespace homeControl.Configuration
{
    public sealed class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}