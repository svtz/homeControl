using System;

namespace homeControl.Peripherals
{
    public interface ISensor
    {
        event EventHandler<SensorEventArgs> SensorActivated;
        event EventHandler<SensorEventArgs> SensorDeactivated;
    }
}