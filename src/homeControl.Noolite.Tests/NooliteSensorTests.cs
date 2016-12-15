using System;
using homeControl.Configuration;
using homeControl.Configuration.Sensors;
using homeControl.Noolite.Adapters;
using homeControl.Noolite.Configuration;
using homeControl.Peripherals;
using Moq;
using ThinkingHome.NooLite.ReceivedData;
using Xunit;

namespace homeControl.Noolite.Tests
{
    public class NooliteSensorTests
    {
        private RX2164ReceivedCommandData CreateCommandData(byte cmd, byte channel)
        {
            var buf = new byte[9];
            buf[2] = channel;
            buf[3] = cmd;
            return new RX2164ReceivedCommandData(buf);
        }

        [Theory]
        [InlineData(2, 1, 0)]
        [InlineData(0, 0, 1)]
        public void TestWhenAdapterReceivedCommand_ThenRaiseEvent(byte command, int expectedActivateCallCount, int expectedDeactivateCallCount)
        {
            var sensorConfig = new NooliteSensorConfig {SensorId = SensorId.NewId(), Channel = 17 };

            var configMock = new Mock<ISensorConfigurationRepository>();
            configMock.Setup(cfg => cfg.GetAllConfigs<NooliteSensorConfig>()).Returns(new[] { sensorConfig });

            var gateMock = new Mock<ISensorGate>(MockBehavior.Strict);
            gateMock.Setup(g => g.OnSensorActivated(sensorConfig.SensorId));
            gateMock.Setup(g => g.OnSensorDeactivated(sensorConfig.SensorId));

            var adapterMock = new Mock<IRX2164Adapter>();
            var sensor = new NooliteSensor(gateMock.Object, adapterMock.Object, configMock.Object);
            sensor.Activate();

            adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(command, sensorConfig.Channel));

            gateMock.Verify(m => m.OnSensorActivated(It.IsAny<SensorId>()), Times.Exactly(expectedActivateCallCount));
            gateMock.Verify(m => m.OnSensorDeactivated(It.IsAny<SensorId>()), Times.Exactly(expectedDeactivateCallCount));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void TestWhenAdapterReceivedCommandWithUnknownChannel_ThenError(byte command)
        {
            var configMock = new Mock<ISensorConfigurationRepository>();

            var adapterMock = new Mock<IRX2164Adapter>();

            var gateMock = new Mock<ISensorGate>(MockBehavior.Strict);
            var sensor = new NooliteSensor(gateMock.Object, adapterMock.Object, configMock.Object);
            sensor.Activate();

            Action act = () => adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(command, 13));

            Assert.Throws<InvalidConfigurationException>(act);
        }
    }
}
