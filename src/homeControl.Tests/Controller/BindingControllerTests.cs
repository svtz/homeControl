using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.ControllerService.Bindings;
using homeControl.Domain;
using homeControl.Domain.Configuration.Bindings;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Switches;
using homeControl.Domain.Repositories;
using Moq;
using Xunit;

namespace homeControl.Tests.Controller
{
    public class BindingControllerTests
    {
        [Fact]
        public async void TestWnehUnknownSensor_ThenDoNothing()
        {
            IReadOnlyCollection<SwitchToSensorBinding> config = new []
            {
                new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() }
            };
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(config));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorActivation(SensorId.NewId());
            await handler.ProcessSensorDeactivation(SensorId.NewId());

            publisherMock.Verify(m => m.SendEvent(It.IsAny<IEvent>()), Times.Never);
        }


        [Fact]
        public async void TestWhenUnknownBinding_ThenError()
        {
            IReadOnlyCollection<SwitchToSensorBinding> config = new []
            {
                new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() }
            };
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(config));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.EnableBinding(SwitchId.NewId(), SensorId.NewId()));
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.DisableBinding(SwitchId.NewId(), SensorId.NewId()));

            publisherMock.Verify(m => m.SendEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [Fact]
        public async void Test_ConsecutiveEnableEvents_AreOk()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var handler = new BindingController(Mock.Of<IEventSender>(), bindingsRepoMock.Object);

            await handler.EnableBinding(config.SwitchId, config.SensorId);
            await handler.EnableBinding(config.SwitchId, config.SensorId);
        }

        [Fact]
        public async void Test_ConsecutiveDisableEvents_AreOk()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var handler = new BindingController(Mock.Of<IEventSender>(), bindingsRepoMock.Object);

            await handler.DisableBinding(config.SwitchId, config.SensorId);
            await handler.DisableBinding(config.SwitchId, config.SensorId);
        }

        [Fact]
        public async void TestBindingEnabledByDefault_TurnOn()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorActivation(config.SensorId);
            
            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public async void TestBindingEnabledByDefault_TurnOff()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }
        
        [Fact]
        public async void TestWhenBindingDeactivated_DoNothing()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.DisableBinding(config.SwitchId, config.SensorId);
            await handler.ProcessSensorActivation(config.SensorId);
            await handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [Fact]
        public async void TestWhenBindingOnOnly_IgnoreOffSensorEvents()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOnly, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorActivation(config.SensorId);
            await handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOnEvent>()), Times.Once);
            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOffEvent>()), Times.Never);
        }
        
        [Fact]
        public async void TestWhenBindingOffOnly_IgnoreOnSensorEvents()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOnly, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorActivation(config.SensorId);
            await handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOnEvent>()), Times.Never);
            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }
        
        //multipleSwitches

        [Fact]
        public async void TestWhenBindingReactivated_ActivationPublishesTurnOn()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.DisableBinding(config.SwitchId, config.SensorId);
            await handler.EnableBinding(config.SwitchId, config.SensorId);
            await handler.ProcessSensorActivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public async void TestWhenBindingReactivated_DeactivationPublishesTurnOff()
        {
            var config = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = SensorId.NewId() };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.DisableBinding(config.SwitchId, config.SensorId);
            await handler.EnableBinding(config.SwitchId, config.SensorId);
            await handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }

        [Fact]
        public async void TestWhenSensorBindToMultipleSwitches_ThenTurnOnThemAll()
        {
            var sensorId = SensorId.NewId();
            var config1 = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = sensorId };
            var config2 = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = sensorId };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config1, config2};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config1.SwitchId)));
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config2.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorActivation(sensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOnEvent>()), Times.Exactly(2));
        }

        [Fact]
        public async void TestWhenSensorBindToMultipleSwitches_ThenTurnOffThemAll()
        {
            var sensorId = SensorId.NewId();
            var config1 = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = sensorId };
            var config2 = new OnOffBinding { Mode = OnOffBindingMode.OnOff, SwitchId = SwitchId.NewId(), SensorId = sensorId };
            IReadOnlyCollection<SwitchToSensorBinding> configCollection = new [] {config1, config2};
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(Task.FromResult(configCollection));
            var publisherMock = new Mock<IEventSender>(MockBehavior.Strict);
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config1.SwitchId)));
            publisherMock.Setup(m => m.SendEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config2.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            await handler.ProcessSensorDeactivation(sensorId);

            publisherMock.Verify(m => m.SendEvent(It.IsAny<TurnOffEvent>()), Times.Exactly(2));
        }
    }
}