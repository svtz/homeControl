using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using homeControl.Client.WPF.ViewModels.Switches;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.Client.WPF.ViewModels
{
    [UsedImplicitly]
    public class ClientViewModel : ViewModelBase
    {
        private readonly AutorunConfigurator _autoRunCfg;
        private readonly ILogger _log;
        private readonly SwitchViewModelsFactory _switchViewModelsFactory;

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public ICommand TrayIconClickCommand { get; }
        public ICommand ExitClickCommand { get; }
        public ICommand ReloadSwitchesCommand { get; }

        public ReadOnlyObservableCollection<SwitchViewModelBase> Switches
            => new ReadOnlyObservableCollection<SwitchViewModelBase>(_switches);
        private readonly ObservableCollection<SwitchViewModelBase> _switches;


        public ClientViewModel(AutorunConfigurator autoRunCfg, ILogger log,
            SwitchViewModelsFactory switchViewModelsFactory)
        {
            Guard.DebugAssertArgumentNotNull(autoRunCfg, nameof(autoRunCfg));
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Guard.DebugAssertArgumentNotNull(switchViewModelsFactory, nameof(switchViewModelsFactory));
                
            _autoRunCfg = autoRunCfg;
            _log = log;
            _switchViewModelsFactory = switchViewModelsFactory;

            _switches = new ObservableCollection<SwitchViewModelBase>();

            ReloadSwitchesCommand = new RelayCommand(DoReloadSwitches, () => !IsBusy);
            ExitClickCommand = new RelayCommand(Exit);
        }

        private void DoReloadSwitches()
        {
            IsBusy = true;
            try
            {
                _log.Debug("Идёт обновление набора переключателей.");
                foreach (var oldSwitch in _switches)
                {
                    _switches.Remove(oldSwitch);
                    (oldSwitch as IDisposable)?.Dispose();
                }

                foreach (var @switch in _switchViewModelsFactory.CreateViewModels())
                {
                    _switches.Add(@switch);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "При обновлении набора переключателей произошла ошибка.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Exit()
        {
            _autoRunCfg.RemoveAutoRun();
            Application.Current.Shutdown();
        }
    }
}
