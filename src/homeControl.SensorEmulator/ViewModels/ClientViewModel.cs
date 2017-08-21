using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using homeControl.SensorEmulator.ViewModels.Sensors;
using JetBrains.Annotations;
using Serilog;

namespace homeControl.SensorEmulator.ViewModels
{
    [UsedImplicitly]
    public class ClientViewModel : ViewModelBase
    {
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

        public ICommand ExitClickCommand { get; }
        public ICommand ReloadSwitchesCommand { get; }

        public ReadOnlyObservableCollection<SensorViewModelBase> Switches
            => new ReadOnlyObservableCollection<SensorViewModelBase>(_switches);
        private readonly ObservableCollection<SensorViewModelBase> _switches;


        public ClientViewModel(ILogger log,
            SwitchViewModelsFactory switchViewModelsFactory)
        {
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Guard.DebugAssertArgumentNotNull(switchViewModelsFactory, nameof(switchViewModelsFactory));
                
            _log = log;
            _switchViewModelsFactory = switchViewModelsFactory;

            _switches = new ObservableCollection<SensorViewModelBase>();

            ReloadSwitchesCommand = new RelayCommand(DoReloadSwitches, () => !IsBusy);
            ExitClickCommand = new RelayCommand(Exit);
        }

        private async void DoReloadSwitches()
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

                foreach (var @switch in await _switchViewModelsFactory.CreateViewModels())
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
            Application.Current.Shutdown();
        }
    }
}
