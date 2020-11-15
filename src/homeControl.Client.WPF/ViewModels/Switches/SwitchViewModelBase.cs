using System;
using System.Linq;
using System.Reactive.Linq;
using GalaSoft.MvvmLight;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Bindings;
using Serilog;

namespace homeControl.Client.WPF.ViewModels.Switches
{
    public abstract class SwitchViewModelBase : ViewModelBase
    {
        private readonly IEventSender _eventSender;
        private readonly IDisposable _automationEventSubscription;
        private readonly SensorId[] _sensors;
        
        protected ILogger Log { get; }
        protected readonly object UserInteractionLock = new object();
        
        public SwitchId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAutomated => _sensors.Any();

        

        private bool _isAutomationEnabled = true;
        public bool IsAutomationEnabled
        {
            get => _isAutomationEnabled;
            set
            {
                if (!IsAutomated)
                    throw new InvalidOperationException("Переключатель не автоматизирован");
                
                _isAutomationEnabled = value;
                
                OnAutomationChangedByUser();
                RaisePropertyChanged();
            }
        }


        protected SwitchViewModelBase(IEventReceiver eventReceiver, IEventSender eventSender, SensorId[] sensors,
            ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            Guard.DebugAssertArgumentNotNull(eventReceiver, nameof(eventReceiver));
            Guard.DebugAssertArgumentNotNull(eventSender, nameof(eventSender));
            Guard.DebugAssertArgumentNotNull(sensors, nameof(sensors));
            
            _eventSender = eventSender;
            _sensors = sensors;
            
            Log = log.ForContext(GetType());

            if (sensors.Any())
            {
                _automationEventSubscription = eventReceiver
                    .ReceiveEvents<AbstractBindingEvent>()
                    .Where(e => e.SwitchId == Id)
                    .ForEachAsync(UpdateAutomationFromEvent);
            }
        }


        private void UpdateAutomationFromEvent(AbstractBindingEvent e)
        {
            Log.Debug("Пользователь изменил автоматизацию.");
        
            if (!IsAutomated)
                throw new InvalidOperationException("Переключатель не автоматизирован");
            
            var newAutomation = e is EnableBindingEvent;
            
            if (_isAutomationEnabled == newAutomation)
                return;

            lock (UserInteractionLock)
            {
                if (_isAutomationEnabled == newAutomation)
                    return;

                _isAutomationEnabled = newAutomation;
                RaisePropertyChanged(() => IsAutomationEnabled);
            }
        }

        private void OnAutomationChangedByUser()
        {
            Log.Debug("Пользователь изменил автоматизацию.");

            if (!IsAutomated)
                throw new InvalidOperationException("Переключатель не автоматизирован");

            foreach (var sensorId in _sensors)
            {
                var @event = _isAutomationEnabled
                    ? (IEvent)new EnableBindingEvent(Id, sensorId)
                    : (IEvent)new DisableBindingEvent(Id, sensorId);
                
                _eventSender.SendEvent(@event);
            }
            
            Log.Debug("События автоматизации отправлены.");
        }
        
        public virtual void Dispose()
        {
            _automationEventSubscription?.Dispose();
        }
    }
}