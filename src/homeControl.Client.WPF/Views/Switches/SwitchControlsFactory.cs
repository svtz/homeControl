using System;
using System.Windows.Controls;
using homeControl.Client.WPF.ViewModels.Switches;

namespace homeControl.Client.WPF.Views.Switches
{
    class SwitchControlsFactory
    {
        public UserControl CreateSwitchControl(object dataContext)
        {
            var gradient = dataContext as GradientSwitchViewModel;
            if (gradient != null)
            {
                return new GradientSwitchControl
                {
                    DataContext = gradient
                };
            }

            var toggle = dataContext as ToggleSwitchViewModel;
            if (toggle != null)
            {
                return new ToggleSwitchControl()
                {
                    DataContext = toggle
                };
            }

            throw new NotImplementedException();
        }
    }
}
