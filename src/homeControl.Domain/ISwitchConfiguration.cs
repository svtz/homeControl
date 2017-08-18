namespace homeControl.Domain
{
    public interface ISwitchConfiguration
    {
        SwitchId SwitchId { get; }
        SwitchKind SwitchKind { get; }
        string Name { get; }
        string Description { get; }
    }
}