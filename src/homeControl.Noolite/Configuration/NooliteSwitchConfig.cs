using homeControl.Configuration.Switches;

namespace homeControl.Noolite.Configuration
{
    internal class NooliteSwitchConfig : ISwitchConfiguration
    {
        public byte Channel { get; set; }
        public SwitchId SwitchId { get; set; }
    }
}