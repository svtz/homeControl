using homeControl.Domain;

namespace homeControl.NooliteService.SwitchController
{
    public interface ISwitchController
    {
        bool CanHandleSwitch(SwitchId switchId);
        void TurnOn(SwitchId switchId);
        void TurnOff(SwitchId switchId);
        void SetPower(SwitchId switchId, double power);
    }
}