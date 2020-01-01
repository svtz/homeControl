using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using homeControl.Client.WPF.ViewModels;
using homeControl.Client.WPF.ViewModels.Switches;
using homeControl.Client.WPF.Views.Switches;

namespace homeControl.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
            Hide();
        }


        #region Прячем/показываем окно

        /// <summary> Клик по трей-иконке </summary>
        private void TrayIconClick(object sender, RoutedEventArgs e)
        {
            Show();
            Activate();
            _clickMousePosition = System.Windows.Forms.Control.MousePosition;
        }

        private System.Drawing.Point _clickMousePosition;
        private bool _positioned = false;
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_positioned) 
            {
                return;
            }
            
            _positioned = true;

            var width = Math.Max(ActualWidth, MinWidth);
            var height = Math.Max(ActualHeight, MinHeight);

            var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice;
            var dpiAwareCursorPos = new Point(_clickMousePosition.X, _clickMousePosition.Y);
            dpiAwareCursorPos = transform?.Transform(dpiAwareCursorPos) ?? dpiAwareCursorPos;

            var suggestedLeft = dpiAwareCursorPos.X - width;
            var suggestedTop = dpiAwareCursorPos.Y - height;

            if (suggestedLeft < 0)
                suggestedLeft = dpiAwareCursorPos.X;
            if (suggestedTop < 0)
                suggestedTop = dpiAwareCursorPos.Y;

            Left = suggestedLeft;
            Top = suggestedTop;
        }

        /// <summary> По крестику - в трей </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width == 0)
            {
                return;
            }

            Left -= e.NewSize.Width - e.PreviousSize.Width;
        }
        
        #endregion

        
        /// <summary> Форма закрылась </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (DataContext is IDisposable disposableCtx)
            {
                disposableCtx.Dispose();
                DataContext = null;
            }
        }
    }
}
