using homeControl.Configuration;

namespace homeControl.Noolite.Configuration
{
    internal class NooliteSwitchConfig : ISwitchConfiguration
    {
        public byte Channel { get; set; }
    }
}