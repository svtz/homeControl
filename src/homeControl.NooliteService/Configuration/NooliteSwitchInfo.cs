using homeControl.Domain;

namespace homeControl.NooliteService.Configuration
{
    internal class NooliteSwitchInfo
    {
        public byte Channel { get; set; }
        public SwitchId SwitchId { get; set; }

        public byte FullPowerLevel { get; set; }
        public byte ZeroPowerLevel { get; set; }
    }
}