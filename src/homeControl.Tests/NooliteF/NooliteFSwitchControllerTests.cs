using System.Threading.Tasks;
using homeControl.Domain;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using homeControl.NooliteService.SwitchController;
using Moq;
using ThinkingHome.NooLite;
using Xunit;

namespace homeControl.Tests.NooliteF
{
    public class NooliteFSwitchControllerTests
    {
        [Fact]
        public void Test_TurnOn_SendsAdapterOnCommand()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>();
            var config = new NooliteFSwitchInfo { Channel = 123, UseF = false };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());
            controller.TurnOn(switchId);

            adapterMock.Verify(adapter => adapter.On(config.Channel), Times.Once);
        }
        
        [Fact]
        public void Test_TurnOnF_SendsAdapterOnFCommand()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>();
            var config = new NooliteFSwitchInfo { Channel = 123, UseF = true };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());
            controller.TurnOn(switchId);

            adapterMock.Verify(adapter => adapter.OnF(config.Channel, null), Times.Once);
        }

        [Fact]
        public void Test_TurnOff_SendsAdapterOffCommand()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>();
            var config = new NooliteFSwitchInfo { Channel = 98, UseF = false };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());
            controller.TurnOff(switchId);

            adapterMock.Verify(adapter => adapter.Off(config.Channel), Times.Once);
        }
        
        [Fact]
        public void Test_TurnOffF_SendsAdapterOffFCommand()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>();
            var config = new NooliteFSwitchInfo { Channel = 98, UseF = true };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());
            controller.TurnOff(switchId);

            adapterMock.Verify(adapter => adapter.OffF(config.Channel, null), Times.Once);
        }

        [Fact]
        public void Test_IfRepoDoesNotContainConfig_ThenCantHandle()
        {
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(It.IsAny<SwitchId>()))
                .Returns(Task.FromResult(false));

            var controller = new NooliteFSwitchController(configRepositoryMock.Object, Mock.Of<IMtrfAdapter>(), new NooliteFSwitchesStatusHolder());
            Assert.False(controller.CanHandleSwitch(SwitchId.NewId()));

            configRepositoryMock.Verify(repo => repo.ContainsConfig(It.IsAny<SwitchId>()), Times.Once);
        }

        [Fact]
        public void Test_IfRepoContainsConfig_ThenCanHandle()
        {
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var switchId = SwitchId.NewId();
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            
            var controller = new NooliteFSwitchController(configRepositoryMock.Object, Mock.Of<IMtrfAdapter>(), new NooliteFSwitchesStatusHolder());
            Assert.True(controller.CanHandleSwitch(switchId));

            configRepositoryMock.Verify(repo => repo.ContainsConfig(switchId), Times.Once);
        }

        [Theory]
        [InlineData(127, 0, 1.0, 127)]
        [InlineData(127, 0, 0.0, 0)]
        [InlineData(127, 0, 0.5, 64)]
        [InlineData(100, 50, 1.0, 100)]
        [InlineData(100, 50, 0.0, 50)]
        [InlineData(100, 50, 0.2, 60)]
        public void Test_SetBrightness_ChangesAdaptersBrightness(byte fullPower, byte zeroPower, double requestedPower, byte expectedLevel)
        {
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var config = new NooliteFSwitchInfo
            {
                SwitchId = SwitchId.NewId(),
                Channel = 98,
                FullPowerLevel = fullPower,
                ZeroPowerLevel = zeroPower,
                UseF = false
            };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(config.SwitchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(config.SwitchId))
                .Returns(Task.FromResult(config));
            var adapterMock = new Mock<IMtrfAdapter>(MockBehavior.Strict);
            adapterMock.Setup(adapter => adapter.SetBrightness(config.Channel, expectedLevel));
            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());

            controller.SetPower(config.SwitchId, requestedPower);

            adapterMock.Verify(adapter => adapter.SetBrightness(config.Channel, expectedLevel), Times.Once);
        }
        
        [Theory]
        [InlineData(127, 0, 1.0, 127)]
        [InlineData(127, 0, 0.0, 0)]
        [InlineData(127, 0, 0.5, 64)]
        [InlineData(100, 50, 1.0, 100)]
        [InlineData(100, 50, 0.0, 50)]
        [InlineData(100, 50, 0.2, 60)]
        public void Test_SetBrightnessF_ChangesAdaptersBrightnessF(byte fullPower, byte zeroPower, double requestedPower, byte expectedLevel)
        {
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var config = new NooliteFSwitchInfo
            {
                SwitchId = SwitchId.NewId(),
                Channel = 98,
                FullPowerLevel = fullPower,
                ZeroPowerLevel = zeroPower,
                UseF = true
            };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(config.SwitchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(config.SwitchId))
                .Returns(Task.FromResult(config));
            var adapterMock = new Mock<IMtrfAdapter>(MockBehavior.Strict);
            adapterMock.Setup(adapter => adapter.SetBrightnessF(config.Channel, expectedLevel, null));
            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, new NooliteFSwitchesStatusHolder());

            controller.SetPower(config.SwitchId, requestedPower);

            adapterMock.Verify(adapter => adapter.SetBrightnessF(config.Channel, expectedLevel, null), Times.Once);
        }

        [Fact]
        public void Test_WhenTurnOnCalled_DontCallAdapterIfStatusAlreadyOn()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>(MockBehavior.Strict);
            var config = new NooliteFSwitchInfo { Channel = 98, UseF = false };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var statusHolder = new NooliteFSwitchesStatusHolder();
            statusHolder.SetStatus(config.Channel, 1, true);
            
            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, statusHolder);
            controller.TurnOn(switchId);

            adapterMock.Verify(adapter => adapter.On(config.Channel), Times.Never);
        }
        
        [Fact]
        public void Test_WhenTurnOffCalled_DontCallAdapterIfStatusAlreadyOff()
        {
            var switchId = SwitchId.NewId();
            var configRepositoryMock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            var adapterMock = new Mock<IMtrfAdapter>(MockBehavior.Strict);
            var config = new NooliteFSwitchInfo { Channel = 98, UseF = false };
            configRepositoryMock
                .Setup(repository => repository.ContainsConfig(switchId))
                .Returns(Task.FromResult(true));
            configRepositoryMock
                .Setup(repository => repository.GetConfig(switchId))
                .Returns(Task.FromResult(config));

            var statusHolder = new NooliteFSwitchesStatusHolder();
            statusHolder.SetStatus(config.Channel, 0, false);
            
            var controller = new NooliteFSwitchController(configRepositoryMock.Object, adapterMock.Object, statusHolder);
            controller.TurnOff(switchId);

            adapterMock.Verify(adapter => adapter.Off(config.Channel), Times.Never);
        }
    }
}
