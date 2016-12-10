using System;
using homeControl.Configuration;
using homeControl.Noolite.Configuration;
using Moq;
using ThinkingHome.NooLite;
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
            var sensorConfig = new NooliteSensorConfig {SensorId = Guid.NewGuid(), Channel = 17 };

            var configMock = new Mock<ISensorConfigurationRepository>();
            configMock.Setup(cfg => cfg.GetAllSensorConfigs<NooliteSensorConfig>()).Returns(new[] { sensorConfig });

            var adapterMock = new Mock<IRX2164Adapter>();
            var sensor = new NooliteSensor(adapterMock.Object, configMock.Object);

            var affectedSensorId = Guid.Empty;
            var activateCallCount = 0;
            sensor.SensorActivated += (s, e) =>
            {
                activateCallCount++;
                affectedSensorId = e.SensorId;
            };
            var deactivateCallCount = 0;
            sensor.SensorDeactivated += (s, e) =>
            {
                deactivateCallCount++;
                affectedSensorId = e.SensorId;
            };


            adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(command, sensorConfig.Channel));

            Assert.Equal(expectedActivateCallCount, activateCallCount);
            Assert.Equal(expectedDeactivateCallCount, deactivateCallCount);
            Assert.Equal(sensorConfig.SensorId, affectedSensorId);
        }

        [Fact]
        public void TestWhenAdapterReceivedCommandWithUnknownChannel_ThenError()
        {
            var configMock = new Mock<ISensorConfigurationRepository>();

            var adapterMock = new Mock<IRX2164Adapter>();
            var sensor = new NooliteSensor(adapterMock.Object, configMock.Object);

            Action act = () => adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(2, 13));

            Assert.Throws<InvalidConfigurationException>(act);
        }
    }
}
