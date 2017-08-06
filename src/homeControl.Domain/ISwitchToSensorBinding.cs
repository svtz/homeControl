namespace homeControl.Domain
{
    public interface ISwitchToSensorBinding
    {
        SwitchId SwitchId { get; }
        SensorId SensorId { get; }
    }
}