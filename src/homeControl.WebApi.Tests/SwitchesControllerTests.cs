using System;
using System.Linq;
using homeControl.Configuration.Sensors;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.Events.Bindings;
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

        private IClientApiConfigurationRepository CreateConfigRepository(params SwitchApiConfig[] configs)
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            configMock.Setup(m => m.GetAll())
                      .Returns(configs);
            configMock.Setup(m => m.TryGetById(It.IsAny<Guid>()))
                      .Returns<Guid>(id => configs.SingleOrDefault(cfg => cfg.ConfigId == id));

            return configMock.Object;
        }

        private TConfig CreateRandomConfig<TConfig>(SwitchKind kind)
            where TConfig : SwitchApiConfig, new()
        {
            var id = Guid.NewGuid();
            var config =  new TConfig
            {
                ConfigId = id,
                Name = id.ToString(),
                Description = $"{kind}: {id}",
                Kind = kind,
                SwitchId = SwitchId.NewId()
            };

            var automatedCfg = config as AutomatedSwitchApiConfig;
            if (automatedCfg != null)
            {
                automatedCfg.SensorId = SensorId.NewId();
            }

            return config;
        }

        private static bool AreEqual(double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }

        [Fact]
        public void TestGetDescriptions_ReturnsConfiguredSwitches()
        {
            var configs = new []
            {
                CreateRandomConfig<SwitchApiConfig>(SwitchKind.ToggleSwitch),
                CreateRandomConfig<SwitchApiConfig>(SwitchKind.GradientSwitch),
                CreateRandomConfig<AutomatedSwitchApiConfig>(SwitchKind.ToggleSwitch),
                CreateRandomConfig<AutomatedSwitchApiConfig>(SwitchKind.GradientSwitch)
            };
            var configRepo = CreateConfigRepository(configs);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

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
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.ToggleSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

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
            var config = CreateRandomConfig<SwitchApiConfig>(switchKind);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

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
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.GradientSwitch);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<AbstractSwitchEvent>(e => e.SwitchId == config.SwitchId && e.GetType() == controlEventType)))
                         .Verifiable();
            publisherMock.Setup(m => m.PublishEvent(It.Is<SetPowerEvent>(e => e.SwitchId == config.SwitchId && AreEqual(e.Power, value))))
                         .Verifiable();
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.SetValue(config.ConfigId, value);

            Assert.IsType<OkResult>(result);
            publisherMock.VerifyAll();
        }

        [Theory]
        [InlineData(typeof(TurnOffEvent), false)]
        [InlineData(typeof(TurnOnEvent), true)]
        public void TestSetValue_ToggleSwitch(Type controlEventType, bool value)
        {
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.ToggleSwitch);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<AbstractSwitchEvent>(e => e.SwitchId == config.SwitchId && e.GetType() == controlEventType)))
                         .Verifiable();
            publisherMock.Setup(m => m.PublishEvent(It.Is<SetPowerEvent>(e => e.SwitchId == config.SwitchId && AreEqual(e.Power, value ? 1 : 0))))
                         .Verifiable();
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.SetValue(config.ConfigId, value);

            Assert.IsType<OkResult>(result);
            publisherMock.VerifyAll();
        }

        [Fact]
        public void TestTurnOn_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.GradientSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.TurnOn(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestTurnOn_PublishesTurnOnEvent(SwitchKind switchKind)
        {
            var config = CreateRandomConfig<SwitchApiConfig>(switchKind);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOnEvent>(e => e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.TurnOn(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOnEvent>()), Times.Once);
        }

        [Fact]
        public void TestTurnOff_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.ToggleSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.TurnOff(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestTurnOff_PublishesTurnOffEvent(SwitchKind switchKind)
        {
            var config = CreateRandomConfig<SwitchApiConfig>(switchKind);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<TurnOffEvent>(e => e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.TurnOff(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<TurnOffEvent>()), Times.Once);
        }

        [Fact]
        public void TestEnableAutomation_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var config = CreateRandomConfig<AutomatedSwitchApiConfig>(SwitchKind.ToggleSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void TestEnableAutomation_ReturnsBadRequest_WhenSwitchIsNotAutomated()
        {
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.GradientSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(config.ConfigId);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestEnableAutomation_PublishesEnableBindingEvent(SwitchKind switchKind)
        {
            var config = CreateRandomConfig<AutomatedSwitchApiConfig>(switchKind);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<EnableBindingEvent>(e => e.SensorId == config.SensorId && e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.EnableAutomation(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<EnableBindingEvent>()), Times.Once);
        }

        [Fact]
        public void TestDisableAutomation_ReturnsBadRequest_WhenRequestedInvalidId()
        {
            var config = CreateRandomConfig<AutomatedSwitchApiConfig>(SwitchKind.GradientSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(Guid.NewGuid());

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void TestDisableAutomation_ReturnsBadRequest_WhenSwitchIsNotAutomated()
        {
            var config = CreateRandomConfig<SwitchApiConfig>(SwitchKind.ToggleSwitch);
            var configRepo = CreateConfigRepository(config);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configRepo, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(config.ConfigId);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(SwitchKind.GradientSwitch)]
        [InlineData(SwitchKind.ToggleSwitch)]
        public void TestDisableAutomation_PublishedDisableBindingEvent(SwitchKind switchKind)
        {
            var config = CreateRandomConfig<AutomatedSwitchApiConfig>(switchKind);
            var configRepo = CreateConfigRepository(config);
            var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            publisherMock.Setup(m => m.PublishEvent(It.Is<DisableBindingEvent>(e => e.SensorId == config.SensorId && e.SwitchId == config.SwitchId)));
            var controller = new SwitchesController(publisherMock.Object, configRepo, _setSwitchValueStrategies);

            var result = controller.DisableAutomation(config.ConfigId);

            Assert.IsType<OkResult>(result);
            publisherMock.Verify(m => m.PublishEvent(It.IsAny<DisableBindingEvent>()), Times.Once);
        }
    }
}
