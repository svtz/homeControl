using System;
using System.Linq;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Sensors;
using homeControl.Events.Switches;
using homeControl.WebApi.Configuration;
using homeControl.WebApi.Controllers;
using homeControl.WebApi.Dto;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace homeControl.WebApi.Tests
{
    public class SwitchesControllerTests
    {
        private static readonly ISetSwitchValueStrategy[] _setSwitchValueStrategies =
        {
            new SetGradientSwitchValueStrategy(),
            new SetToggleSwitchValueStrategy(),
        };


        [Fact]
        public void TestGetDescriptions_ReturnsConfiguredSwitches()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var configs = new[]
            {
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId()
                },
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch2",
                    Description = "Description2",
                    Kind = SwitchKind.GradientSwitch,
                    SwitchId = SwitchId.NewId()
                },
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch3-automated",
                    Description = "Description3",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId()
                },
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch4-automated",
                    Description = "Description4",
                    Kind = SwitchKind.GradientSwitch,
                    SwitchId = SwitchId.NewId()
                }
            };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(configs);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var switches = controller.GetDescriptions();

            Assert.Equal(configs.Length, switches.Length);
            Assert.Equal(configs.Select(c => c.Name), switches.Select(s => s.Name));
            Assert.Equal(configs.Select(c => c.Description), switches.Select(s => s.Description));
            Assert.Equal(configs.Select(c => c.ConfigId), switches.Select(s => s.Id));
            Assert.Equal(configs.Select(c => c.Kind), switches.Select(s => s.Kind));
            Assert.Equal(configs.Select(c => c is AutomatedSwitchApiConfig ? SwitchAutomation.Supported : SwitchAutomation.None), 
                         switches.Select(s => s.Automation));
        }

        [Fact]
        public void TestSetValue_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new [] {config});
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.SetValue(Guid.NewGuid(), new object());
            
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.ToggleSwitch, 0.1d)]
        [InlineData(SwitchKind.GradientSwitch, false)]
        [InlineData(SwitchKind.GradientSwitch, 1.1d)]
        [InlineData(SwitchKind.GradientSwitch, -0.1d)]
        public void TestSetValue_ReturnsBadRequest_WhenValueTypeDoesNotSwitchKind(SwitchKind switchKind, object value)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = switchKind,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new [] {config});
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.SetValue(config.ConfigId, value);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(typeof(TurnOffEvent), 0.0d)]
        [InlineData(typeof(TurnOnEvent), 0.001d)]
        [InlineData(typeof(TurnOnEvent), 0.7d)]
        [InlineData(typeof(TurnOnEvent), 1.0d)]
        public void TestSetValue_GradientSwitch(Type controlEventType, double value)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.GradientSwitch,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<AbstractSwitchEvent>(e => e.SwitchId == config.SwitchId && e.GetType() == controlEventType)))
                         .Verifiable();
            publisherMock.Setup(m => m.PublishEvent(It.Is<SetPowerEvent>(e => e.SwitchId == config.SwitchId && AreEqual(e.Power, value))))
                         .Verifiable();
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.SetValue(config.ConfigId, value);

            Assert.IsType<OkResult>(result);
            publisherMock.VerifyAll();
        }

        [Theory]
        [InlineData(typeof(TurnOffEvent), false)]
        [InlineData(typeof(TurnOnEvent), true)]
        public void TestSetValue_ToggleSwitch(Type controlEventType, bool value)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<AbstractSwitchEvent>(e => e.SwitchId == config.SwitchId && e.GetType() == controlEventType)))
                         .Verifiable();
            publisherMock.Setup(m => m.PublishEvent(It.Is<SetPowerEvent>(e => e.SwitchId == config.SwitchId && AreEqual(e.Power, value ? 1 : 0))))
                         .Verifiable();
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.SetValue(config.ConfigId, value);

            Assert.IsType<OkResult>(result);
            publisherMock.VerifyAll();
        }

        private static bool AreEqual(double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }

        [Fact]
        public void TestTurnOn_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.GradientSwitch,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.TurnOn(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestTurnOn_PublishesTurnOnEvent(SwitchKind switchKind)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = switchKind,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.TurnOn(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public void TestTurnOff_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.TurnOff(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestTurnOff_PublishesTurnOffEvent(SwitchKind switchKind)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = switchKind,
                    SwitchId = SwitchId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.TurnOff(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }

        [Fact]
        public void TestEnableAutomation_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId(),
                    SensorId = SensorId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new SwitchApiConfig[] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void TestEnableAutomation_ReturnsBadRequest_WhenSwitchIsNotAutomated()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId(),
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new [] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(config.ConfigId);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestEnableAutomation_PublishedEnableAutomationEvent(SwitchKind switchKind)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = switchKind,
                    SwitchId = SwitchId.NewId(),
                    SensorId = SensorId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new SwitchApiConfig[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<EnableSensorAutomationEvent>(e => e.SensorId == config.SensorId)));
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<EnableSensorAutomationEvent>()), Times.Once);
        }

        [Fact]
        public void TestDisableAutomation_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId(),
                    SensorId = SensorId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new SwitchApiConfig[] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void TestDisableAutomation_ReturnsBadRequest_WhenSwitchIsNotAutomated()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new SwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = SwitchKind.ToggleSwitch,
                    SwitchId = SwitchId.NewId(),
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new[] { config });
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(config.ConfigId);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestDisableAutomation_PublishedEnableAutomationEvent(SwitchKind switchKind)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var config =
                new AutomatedSwitchApiConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    Kind = switchKind,
                    SwitchId = SwitchId.NewId(),
                    SensorId = SensorId.NewId()
                };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(new SwitchApiConfig[] { config });
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<DisableSensorAutomationEvent>(e => e.SensorId == config.SensorId)));
            var controller = new SwitchesController(publisherMock.Object, configMock.Object, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<DisableSensorAutomationEvent>()), Times.Once);
        }
    }
}
