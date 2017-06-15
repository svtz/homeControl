using homeControl.Configuration.Switches;
using homeControl.Noolite;
using homeControl.Noolite.Adapters;
using homeControl.Noolite.Configuration;
using homeControl.NooliteService;
using Moq;
using ThinkingHome.NooLite;
using Xunit;

namespace homeControl.Tests.Noolite
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

        [Theory]
        [InlineData(127, 0, 1.0, 127)]
        [InlineData(127, 0, 0.0, 0)]
        [InlineData(127, 0, 0.5, 64)]
        [InlineData(100, 50, 1.0, 100)]
        [InlineData(100, 50, 0.0, 50)]
        [InlineData(100, 50, 0.2, 60)]
        public void Test_SetPower_ChangesAdapterLevel(byte fullPower, byte zeroPower, double requestedPower, byte expectedLevel)
        {
            var configRepositoryMock = new Mock<ISwitchConfigurationRepository>(MockBehavior.Strict);
            var config = new NooliteSwitchConfig
            {
                SwitchId = SwitchId.NewId(),
                Channel = 98,
                FullPowerLevel = fullPower,
                ZeroPowerLevel = zeroPower
            };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig<NooliteSwitchConfig>(config.SwitchId))
                .Returns(true);
            configRepositoryMock
                .Setup(repository => repository.GetConfig<NooliteSwitchConfig>(config.SwitchId))
                .Returns(config);
            var adapterMock = new Mock<IPC11XXAdapter>(MockBehavior.Strict);
            adapterMock.Setup(adapter => adapter.SendCommand(PC11XXCommand.SetLevel, config.Channel, expectedLevel));
            var controller = new NooliteSwitchController(configRepositoryMock.Object, adapterMock.Object);

            controller.SetPower(config.SwitchId, requestedPower);

            adapterMock.Verify(adapter => adapter.SendCommand(PC11XXCommand.SetLevel, config.Channel, expectedLevel), Times.Once);
        }
    }
}
