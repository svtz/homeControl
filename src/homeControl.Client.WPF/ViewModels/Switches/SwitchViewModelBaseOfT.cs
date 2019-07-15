using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
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

                lock (UserInteractionLock)
                {
                    if (Equals(_value, value))
                        return;

                    _value = value;

                    OnValueChangedByUser();
                    RaisePropertyChanged();
                }
            }
        }
        
        public ICommand MouseWheelDown { get; }
        public ICommand MouseWheelUp { get; }
        public ICommand SetMaximum { get; }
        public ICommand SetMinimum { get; }


        protected SwitchViewModelBaseOfT(IEventSource eventSource,
            IEventSender eventSender,
            SensorId[] sensors,
            ILogger log)
            : base(eventSource, eventSender, sensors, log)
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
                .Where(e => e.SwitchId == Id)
                .ForEachAsync(UpdateValueFromEvent);

            Log.Debug("Вьюмодель переключателя создана.");
        }

        private void OnSetMinimum()
        {
            Value = GetMinimumValue();
        }

        private void OnSetMaximum()
        {
            Value = GetMaximumValue();
        }

        private void OnMouseWheelUp()
        {
            Value = GetMouseWheelUpValue();
        }

        private void OnMouseWheelDown()
        {
            Value = GetMouseWheelDownValue();
        }

        protected abstract T GetMinimumValue();
        protected abstract T GetMaximumValue();
        protected abstract T GetMouseWheelUpValue();
        protected abstract T GetMouseWheelDownValue();
        protected abstract T GetValueFromEvent(AbstractSwitchEvent e);
        protected abstract IEnumerable<AbstractSwitchEvent> GetEventsFromValue(T value);

        private void UpdateValueFromEvent(AbstractSwitchEvent e)
        {
            var newValue = GetValueFromEvent(e);
            if (Equals(newValue, _value))
                return;

            lock (UserInteractionLock)
            {
                if (Equals(newValue, _value))
                    return;

                _value = newValue;
                RaisePropertyChanged(() => Value);
            }
        }
        
        private void OnValueChangedByUser()
        {
            Log.Debug("Пользователь изменил значение.");

            var events = GetEventsFromValue(Value);
            foreach (var @event in events)
            {
                _eventSender.SendEvent(@event);
            }

            Log.Debug("События значения отправлены.");
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _eventSubscription.Dispose();
        }
    }
}
