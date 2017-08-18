using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    public abstract class SwitchViewModelBaseOfT<T> : SwitchViewModelBase, IDisposable
    {
        private readonly IEventSender _eventSender;

        private T _value;
        private readonly IDisposable _eventSubscription;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                
                _value = value;
                OnValueChanged();
                RaisePropertyChanged();
            }
        }

        public ICommand MouseWheelDown { get; }
        public ICommand MouseWheelUp { get; }
        public ICommand SetMaximum { get; }
        public ICommand SetMinimum { get; }


        protected SwitchViewModelBaseOfT(
            IEventSource eventSource,
            IEventSender eventSender,
            ILogger log)
            : base(log)
        {
            Guard.DebugAssertArgumentNotNull(eventSource, nameof(eventSource));
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));

            MouseWheelDown = new RelayCommand(OnMouseWheelDown);
            MouseWheelUp = new RelayCommand(OnMouseWheelUp);
            SetMaximum = new RelayCommand(OnSetMaximum);
            SetMinimum = new RelayCommand(OnSetMinimum);
            _eventSender = eventSender;

            _eventSubscription = eventSource
                .ReceiveEvents<AbstractSwitchEvent>()
                .ForEachAsync(SetEvent);

            Log.Debug("Вьюмодель переключателя создана.");
        }

        protected abstract void OnSetMinimum();
        protected abstract void OnSetMaximum();
        protected abstract void OnMouseWheelUp();
        protected abstract void OnMouseWheelDown();

        protected abstract void SetEvent(AbstractSwitchEvent e);
        protected abstract IEnumerable<AbstractSwitchEvent> GetEvent(T value);

        private void OnValueChanged()
        {
            Log.Debug("Пользователь изменил значение.");

            var events = GetEvent(Value);
            foreach (var @event in events)
            {
                _eventSender.SendEvent(@event);
            }
            Log.Debug("События отправлены.");
        }

        public void Dispose()
        {
            _eventSubscription?.Dispose();
        }
    }
}
