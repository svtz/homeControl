using System.Windows.Input;

namespace homeControl.Client.WPF.Views.Switches.MouseWheel
{
    internal class MouseWheelGesture : MouseGesture
    {
        public MouseWheelDirection Direction { get; set; }

        public MouseWheelGesture(ModifierKeys keys, MouseWheelDirection direction)
            : base(MouseAction.WheelClick, keys)
        {
            Direction = direction;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var args = inputEventArgs as MouseWheelEventArgs;
            if (args == null)
                return false;
            if (!base.Matches(targetElement, inputEventArgs))
                return false;
            if (Direction == MouseWheelDirection.Up && args.Delta > 0
                || Direction == MouseWheelDirection.Down && args.Delta < 0)
            {
                inputEventArgs.Handled = true;
                return true;
            }

            return false;
        }

    }
}