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
            var cursorPos = System.Windows.Forms.Control.MousePosition;

            var width = Math.Max(ActualWidth, MinWidth);
            var heigth = Math.Max(ActualHeight, MinHeight);

            var suggestedLeft = cursorPos.X - width;
            var suggestedTop = cursorPos.Y - heigth;

            if (suggestedLeft < 0)
                suggestedLeft = cursorPos.X;
            if (suggestedTop < 0)
                suggestedTop = cursorPos.Y;

            Left = suggestedLeft;
            Top = suggestedTop;

            Show();
            Activate();
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
