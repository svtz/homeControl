using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteService;
using homeControl.NooliteService.SwitchController;
using Moq;
using Serilog.Core;
using Xunit;

namespace homeControl.Tests.Noolite
{
    public class SwitchEventsProcessorTests
    {
        private static Task Act(ISwitchController switchCtrl, params AbstractSwitchEvent[] events)
        {
            return Act(switchCtrl, events.AsEnumerable());
        }        
        
        private static async Task Act(ISwitchController switchCtrl, IEnumerable<AbstractSwitchEvent> events)
        {
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            var eventSource = events.ToObservable();
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(eventSource);
            
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromMilliseconds(500000));

                using (var handler = new SwitchEventsProcessor(switchCtrl, eventsSourceMock.Object, TestLoggerHolder.Logger.ForContext<SwitchEventsProcessor>()))
                {
                    handler.RunAsync(cts.Token);
                    await handler.Completion(cts.Token);
                }
            }
        }
        
        [Fact]
        public async void TestTurnOn()
        {
            var switchId = SwitchId.NewId();
            var onEvent = new TurnSwitchOnEvent(switchId);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.TurnOn(switchId));

            await Act(switchControllerMock.Object, onEvent);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOn(switchId), Times.Once);
        }

        [Fact]
        public async void TestTurnOff()
        {
            var switchId = SwitchId.NewId();
            var offEvent = new TurnSwitchOffEvent(switchId);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.TurnOff(switchId));
            
            await Act(switchControllerMock.Object, offEvent);
            
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOff(switchId), Times.Once);
        }

        [Fact]
        public async void TestSetPower()
        {
            var switchId = SwitchId.NewId();
            const double power = 0.4;
            var powerEvent = new SetSwitchPowerEvent(switchId, power);
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.SetPower(switchId, power));
            
            await Act(switchControllerMock.Object, powerEvent);
            
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(switchId, power), Times.Once);
        }

        [Fact]
        public async void TestWhenConsecutiveSetPowerEventsThenUseLatest()
        {
            var switchId = SwitchId.NewId();
            var events = Enumerable
                .Range(0, 10)
                .Select(v => new SetSwitchPowerEvent(switchId, (double) v / 10))
                .ToArray();
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.SetPower(switchId, events.Last().Power));
       
            await Act(switchControllerMock.Object, events.Cast<AbstractSwitchEvent>());
       
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(switchId, events.Last().Power), Times.Once);
        }
        
        [Fact]
        public async void TestWhenConsecutiveOnOffEventsAndLastIsOffThenUseLatestOff()
        {
            var switchId = SwitchId.NewId();
            var events = new AbstractSwitchEvent[]
            {
                new TurnSwitchOnEvent(switchId), 
                new TurnSwitchOnEvent(switchId),
                new TurnSwitchOffEvent(switchId), 
                new TurnSwitchOffEvent(switchId),
                new TurnSwitchOnEvent(switchId),
                new TurnSwitchOffEvent(switchId) 
            };
                
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.TurnOff(switchId));
       
            await Act(switchControllerMock.Object, events);
       
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOff(switchId), Times.Once);
        }
        
        [Fact]
        public async void TestWhenConsecutiveOnOffEventsAndLastIsOnThenUseLatestOn()
        {
            var switchId = SwitchId.NewId();
            var events = new AbstractSwitchEvent[]
            {
                new TurnSwitchOnEvent(switchId), 
                new TurnSwitchOffEvent(switchId), 
                new TurnSwitchOffEvent(switchId),
                new TurnSwitchOnEvent(switchId),
                new TurnSwitchOnEvent(switchId),
                new TurnSwitchOnEvent(switchId),
                new TurnSwitchOffEvent(switchId), 
                new TurnSwitchOnEvent(switchId),
            };
                
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.TurnOn(switchId));
       
            await Act(switchControllerMock.Object, events);
       
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOn(switchId), Times.Once);
        }
        
        [Fact]
        public async void TestWhenMixedEventTypesThenProcessEachTypeInRightOrder()
        {
            var switchId = SwitchId.NewId();
            const double power = 0.5;
            var events = new AbstractSwitchEvent[]
            {
                new TurnSwitchOnEvent(switchId), 
                new SetSwitchPowerEvent(switchId, power),
                new TurnSwitchOffEvent(switchId), 
            };
                
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            var s = new MockSequence();
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId)).Returns(true);
            switchControllerMock.InSequence(s).Setup(ctrl => ctrl.TurnOn(switchId));
            switchControllerMock.InSequence(s).Setup(ctrl => ctrl.SetPower(switchId, power));
            switchControllerMock.InSequence(s).Setup(ctrl => ctrl.TurnOff(switchId));
       
            await Act(switchControllerMock.Object, events);
       
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId), Times.Exactly(3));
            switchControllerMock.Verify(sc => sc.TurnOn(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOff(switchId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(switchId, power), Times.Once);
        }
        
        [Fact]
        public async void TestWhenDifferentSwitchesThenProcessThemSeparately()
        {
            var switchId1 = SwitchId.NewId();
            var switchId2 = SwitchId.NewId();
            var events = new AbstractSwitchEvent[]
            {
                new TurnSwitchOnEvent(switchId1), 
                new TurnSwitchOnEvent(switchId2), 
            };
                
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId1)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(switchId2)).Returns(true);
            switchControllerMock.Setup(ctrl => ctrl.TurnOn(switchId1));
            switchControllerMock.Setup(ctrl => ctrl.TurnOn(switchId2));
       
            await Act(switchControllerMock.Object, events);
       
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId1), Times.Once);
            switchControllerMock.Verify(sc => sc.CanHandleSwitch(switchId2), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOn(switchId1), Times.Once);
            switchControllerMock.Verify(sc => sc.TurnOn(switchId2), Times.Once);
        }
        
               
        // ReSharper disable once MemberCanBePrivate.Global
        // MemberData must reference a public member 
        public static IEnumerable<object[]> AbstractSwitchEvents =>
           new[]
           {
               new object[] { new TurnSwitchOnEvent(SwitchId.NewId()), }, 
               new object[] { new TurnSwitchOffEvent(SwitchId.NewId()), }, 
               new object[] { new SetSwitchPowerEvent(SwitchId.NewId(), 0.333), }, 
           };
       
        [Theory]
        [MemberData(nameof(AbstractSwitchEvents))]
        public async void Test_WhenUnknownSwitch_ThenDoNothing(AbstractSwitchEvent @event)
        {
            var switchControllerMock = new Mock<ISwitchController>(MockBehavior.Strict);
            switchControllerMock.Setup(ctrl => ctrl.CanHandleSwitch(It.IsAny<SwitchId>())).Returns(false);
            var eventsSourceMock = new Mock<IEventSource>(MockBehavior.Strict);
            eventsSourceMock.Setup(e => e.ReceiveEvents<AbstractSwitchEvent>()).Returns(Observable.Repeat(@event, 1));

            await Act(switchControllerMock.Object, @event);

            switchControllerMock.Verify(sc => sc.CanHandleSwitch(@event.SwitchId), Times.Once);
            switchControllerMock.Verify(sc => sc.SetPower(It.IsAny<SwitchId>(), It.IsAny<double>()), Times.Never);
            switchControllerMock.Verify(sc => sc.TurnOff(It.IsAny<SwitchId>()), Times.Never);
            switchControllerMock.Verify(sc => sc.TurnOn(It.IsAny<SwitchId>()), Times.Never);
        }
    }
}
