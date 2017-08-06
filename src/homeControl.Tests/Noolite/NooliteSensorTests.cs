using System;
using homeControl.Configuration;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Repositories;
using homeControl.NooliteService;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using Moq;
using Serilog;
using ThinkingHome.NooLite.ReceivedData;
using Xunit;

namespace homeControl.Tests.Noolite
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
            configMock.Setup(cfg => cfg.GetAll<NooliteSensorConfig>()).Returns(new[] { sensorConfig });

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));
            gateMock.Setup(g => g.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));

            var adapterMock = new Mock<IRX2164Adapter>();
            var sensor = new NooliteSensor(gateMock.Object, adapterMock.Object, configMock.Object, Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(command, sensorConfig.Channel));

            gateMock.Verify(m => m.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedActivateCallCount));
            gateMock.Verify(m => m.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedDeactivateCallCount));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void TestWhenAdapterReceivedCommandWithUnknownChannel_ThenError(byte command)
        {
            var configMock = new Mock<ISensorConfigurationRepository>();

            var adapterMock = new Mock<IRX2164Adapter>();

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            var sensor = new NooliteSensor(gateMock.Object, adapterMock.Object, configMock.Object, Mock.Of<ILogger>());
            sensor.Activate();

            void Act() => adapterMock.Raise(ad => ad.CommandReceived += null, CreateCommandData(command, 13));

            Assert.Throws<InvalidConfigurationException>((Action)Act);
        }
    }
}
