using System;
using homeControl.Configuration.Bindings;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Bindings;
using homeControl.Events.Bindings.Configuration;
using homeControl.Events.Switches;
using Moq;
using Xunit;

namespace homeControl.Events.Tests
{
    public class BindingControllerTests
    {
        [Fact]
        public void TestWnehUnknownSensor_ThenDoNothing()
        {
            var config = new ISwitchToSensorBinding[]
            {
                new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId())
            };
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.ProcessSensorActivation(SensorId.NewId());
            handler.ProcessSensorDeactivation(SensorId.NewId());

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<IEvent>()), Times.Never);
        }


        [Fact]
        public void TestWnehUnknownBinding_ThenError()
        {
            var config = new ISwitchToSensorBinding[]
            {
                new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId())
            };
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            Assert.Throws<InvalidOperationException>(() => handler.EnableBinding(SwitchId.NewId(), SensorId.NewId()));
            Assert.Throws<InvalidOperationException>(() => handler.DisableBinding(SwitchId.NewId(), SensorId.NewId()));

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<IEvent>()), Times.Never);
        }

        [Fact]
        public void Test_ConsecutiveEnableEvents_AreOk()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var handler = new BindingController(Mock.Of<IEventPublisher>(), bindingsRepoMock.Object);

            handler.EnableBinding(config.SwitchId, config.SensorId);
            handler.EnableBinding(config.SwitchId, config.SensorId);
        }

        [Fact]
        public void Test_ConsecutiveDisableEvents_AreOk()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var handler = new BindingController(Mock.Of<IEventPublisher>(), bindingsRepoMock.Object);

            handler.DisableBinding(config.SwitchId, config.SensorId);
            handler.DisableBinding(config.SwitchId, config.SensorId);
        }

        [Fact]
        public void TestBindingEnabledByDefault_TurnOn()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.ProcessSensorActivation(config.SensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public void TestBindingEnabledByDefault_TurnOff()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }
        
        [Fact]
        public void TestWhenBindingDeactivated_DoNothing()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.DisableBinding(config.SwitchId, config.SensorId);
            handler.ProcessSensorActivation(config.SensorId);
            handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<IEvent>()), Times.Never);
        }

        //multipleSwitches

        [Fact]
        public void TestWhenBindingReactivated_ActivationPublishesTurnOn()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.DisableBinding(config.SwitchId, config.SensorId);
            handler.EnableBinding(config.SwitchId, config.SensorId);
            handler.ProcessSensorActivation(config.SensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public void TestWhenBindingReactivated_DeactivationPublishesTurnOff()
        {
            var config = new SwitchToSensorBinding(SwitchId.NewId(), SensorId.NewId());
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.DisableBinding(config.SwitchId, config.SensorId);
            handler.EnableBinding(config.SwitchId, config.SensorId);
            handler.ProcessSensorDeactivation(config.SensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }

        [Fact]
        public void TestWhenSensorBindToMultipleSwitches_ThenTurnOnThemAll()
        {
            var sensorId = SensorId.NewId();
            var config1 = new SwitchToSensorBinding(SwitchId.NewId(), sensorId);
            var config2 = new SwitchToSensorBinding(SwitchId.NewId(), sensorId);
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config1, config2 });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config1.SwitchId)));
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config2.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.ProcessSensorActivation(sensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOnEvent>()), Times.Exactly(2));
        }

        [Fact]
        public void TestWhenSensorBindToMultipleSwitches_ThenTurnOffThemAll()
        {
            var sensorId = SensorId.NewId();
            var config1 = new SwitchToSensorBinding(SwitchId.NewId(), sensorId);
            var config2 = new SwitchToSensorBinding(SwitchId.NewId(), sensorId);
            var bindingsRepoMock = new Mock<ISwitchToSensorBindingsRepository>(MockBehavior.Strict);
            bindingsRepoMock.Setup(m => m.GetAll()).Returns(new ISwitchToSensorBinding[] { config1, config2 });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config1.SwitchId)));
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config2.SwitchId)));
            var handler = new BindingController(publisherMock.Object, bindingsRepoMock.Object);

            handler.ProcessSensorDeactivation(sensorId);

            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOffEvent>()), Times.Exactly(2));
        }
    }
}