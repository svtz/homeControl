using homeControl.Domain;

namespace homeControl.NooliteService.Configuration
{
    internal class NooliteSwitchConfig : ISwitchConfiguration
    {
        public byte Channel { get; set; }
        public SwitchId SwitchId { get; set; }

        public SwitchKind SwitchKind { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public byte FullPowerLevel { get; set; }
        public byte ZeroPowerLevel { get; set; }
    }
}