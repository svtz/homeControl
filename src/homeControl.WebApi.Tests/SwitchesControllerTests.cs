using System;
using System.Linq;
using homeControl.Configuration.Switches;
using homeControl.Core;
using homeControl.WebApi.Configuration;
using homeControl.WebApi.Controllers;
using homeControl.WebApi.Dto;
using Moq;
using Xunit;

namespace homeControl.WebApi.Tests
{
    public class SwitchesControllerTests
    {
        [Fact]
        public void TestGetDescriptions_ReturnsConfiguredSwitches()
        {
            var configMock = new Mock<IClientApiConfigurationRepository>();
            var configs = new[]
            {
                new SwitchApiConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Switch1",
                    Description = "Description1",
                    SwitchId = SwitchId.NewId(),
                    Kind = SwitchKind.ToggleSwitch
                },
                new SwitchApiConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Switch2",
                    Description = "Description2",
                    SwitchId = SwitchId.NewId(),
                    Kind = SwitchKind.GradientSwitch
                },
                new AutomatedSwitchApiConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Switch3-automated",
                    Description = "Description3",
                    SwitchId = SwitchId.NewId(),
                    Kind = SwitchKind.ToggleSwitch
                },
                new AutomatedSwitchApiConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Switch4-automated",
                    Description = "Description4",
                    SwitchId = SwitchId.NewId(),
                    Kind = SwitchKind.GradientSwitch
                }
            };
            configMock.Setup(m => m.GetClientApiConfig()).Returns(configs);
            var controller = new SwitchesController(Mock.Of<IEventPublisher>(), configMock.Object);

            var switches = controller.GetDescriptions();

            Assert.Equal(configs.Length, switches.Length);
            Assert.Equal(configs.Select(c => c.Name), switches.Select(s => s.Name));
            Assert.Equal(configs.Select(c => c.Description), switches.Select(s => s.Description));
            Assert.Equal(configs.Select(c => c.Id), switches.Select(s => s.Id));
            Assert.Equal(configs.Select(c => c.Kind), switches.Select(s => s.Kind));
            Assert.Equal(configs.Select(c => c is AutomatedSwitchApiConfig ? SwitchAutomation.Supported : SwitchAutomation.None), 
                         switches.Select(s => s.Automation));
        }
    }
}
