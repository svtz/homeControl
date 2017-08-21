using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace homeControl.SensorEmulator.Views.Sensors.MouseWheel
{
    public class MouseWheel : MarkupExtension
    {
        public MouseWheelDirection Direction { get; set; }
        public ModifierKeys Keys { get; set; }

        public MouseWheel()
        {
            Keys = ModifierKeys.None;
            Direction = MouseWheelDirection.Any;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new MouseWheelGesture(Keys, Direction);
        }
    }
}