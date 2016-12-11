using homeControl.Configuration.Switches;
using homeControl.Noolite.Adapters;
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
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IPC11XXAdapter>();
            var config = new NooliteSwitchConfig { Channel = 123 };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig<NooliteSwitchConfig>(switchId))
                .Returns(true);
            configRepositoryMock
                .Setup(repository => repository.GetConfig<NooliteSwitchConfig>(switchId))
                .Returns(config);

            var controller = new NooliteSwitchController(configRepositoryMock.Object, adapterMock.Object);
            controller.TurnOn(switchId);

            adapterMock.Verify(adapter => adapter.SendCommand(PC11XXCommand.On, config.Channel, 0), Times.Once);
        }

        [Fact]
        public void Test_TurnOff_SendsAdapterOffCommand()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IPC11XXAdapter>();
            var config = new NooliteSwitchConfig { Channel = 98 };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig<NooliteSwitchConfig>(switchId))
                .Returns(true);
            configRepositoryMock
                .Setup(repository => repository.GetConfig<NooliteSwitchConfig>(switchId))
                .Returns(config);

            var controller = new NooliteSwitchController(configRepositoryMock.Object, adapterMock.Object);
            controller.TurnOff(switchId);

            adapterMock.Verify(adapter => adapter.SendCommand(PC11XXCommand.Off, config.Channel, 0), Times.Once);
        }

        [Fact]
        public void Test_IfRepoDoesNotContainConfig_ThenCantHandle()
        {
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>(MockBehavior.Strict);
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig<NooliteSwitchConfig>(It.IsAny<SwitchId>()))
                .Returns(false);

            var controller = new NooliteSwitchController(configRepositoryMock.Object, Mock.Of<IPC11XXAdapter>());
            Assert.False(controller.CanHandleSwitch(SwitchId.NewId()));

            configRepositoryMock.Verify(repo => repo.ContainsConfig<NooliteSwitchConfig>(It.IsAny<SwitchId>()), Times.Once);
        }

        [Fact]
        public void Test_IfRepoContainsConfig_ThenCanHandle()
        {
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>(MockBehavior.Strict);
            var switchId = SwitchId.NewId();
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig<NooliteSwitchConfig>(switchId))
                .Returns(true);
            
            var controller = new NooliteSwitchController(configRepositoryMock.Object, Mock.Of<IPC11XXAdapter>());
            Assert.True(controller.CanHandleSwitch(switchId));

            configRepositoryMock.Verify(repo => repo.ContainsConfig<NooliteSwitchConfig>(switchId), Times.Once);
        }
    }
}
