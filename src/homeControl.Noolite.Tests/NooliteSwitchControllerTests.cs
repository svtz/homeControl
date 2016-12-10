using System;
using homeControl.Configuration;
using homeControl.Noolite.Configuration;
using Moq;
using ThinkingHome.NooLite;
using Xunit;

namespace homeControl.Noolite.Tests
{
    public class NooliteSwitchControllerTests
    {
        [Fact]
        public void Test_TurnOn_SendsAdapterOnCommand()
        {
            var switchId = Guid.NewGuid();
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>();
            var adapterMock = new Mock<IPC11XXAdapter>();
            var config = new NooliteSwitchConfig { Channel = 123 };
            configRepositoryMock
                .Setup(repository => repository.GetSwicthConfig<NooliteSwitchConfig>(switchId))
                .Returns(config);

            var controller = new NooliteSwitchController(configRepositoryMock.Object, adapterMock.Object);
            controller.TurnOn(switchId);

            adapterMock.Verify(adapter => adapter.SendCommand(PC11XXCommand.On, config.Channel, 0), Times.Once);
        }

        [Fact]
        public void Test_TurnOff_SendsAdapterOffCommand()
        {
            var switchId = Guid.NewGuid();
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>();
            var adapterMock = new Mock<IPC11XXAdapter>();
            var config = new NooliteSwitchConfig { Channel = 98 };
            configRepositoryMock
                .Setup(repository => repository.GetSwicthConfig<NooliteSwitchConfig>(switchId))
                .Returns(config);

            var controller = new NooliteSwitchController(configRepositoryMock.Object, adapterMock.Object);
            controller.TurnOff(switchId);

            adapterMock.Verify(adapter => adapter.SendCommand(PC11XXCommand.Off, config.Channel, 0), Times.Once);
        }
    }
}
