using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.NooliteF;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteService;
using homeControl.NooliteService.Adapters;
using homeControl.NooliteService.Configuration;
using Moq;
using Serilog;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Internal;
using Xunit;

namespace homeControl.Tests.NooliteF
{
    public class NooliteFSensorTests
    {
        private ReceivedData CreateCommandData(MTRFXXCommand cmd, byte channel)
        {
            var buf = new byte[17];
            buf[0] = 173;
            buf[16] = 174; 
            buf[4] = channel;
            buf[5] = (byte)cmd;
            return new ReceivedData(buf);
        }

        [Theory]
        [InlineData(MTRFXXCommand.On, 1, 0)]
        [InlineData(MTRFXXCommand.Off, 0, 1)]
        public void TestWhenAdapterReceivedCommand_ThenRaiseEvent(MTRFXXCommand command, int expectedActivateCallCount, int expectedDeactivateCallCount)
        {
            var sensorConfig = new NooliteFSensorInfo {SensorId = SensorId.NewId(), Channel = 17 };
            var configMock = new Mock<INooliteFSensorInfoRepository>();
            configMock.Setup(cfg => cfg.GetAll()).Returns(Task.FromResult<IReadOnlyCollection<NooliteFSensorInfo>>(new[] { sensorConfig }));

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));
            gateMock.Setup(g => g.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, configMock.Object, Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(command, sensorConfig.Channel));

            gateMock.Verify(m => m.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedActivateCallCount));
            gateMock.Verify(m => m.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedDeactivateCallCount));
        }

        [Theory]
        [InlineData(MTRFXXCommand.Off)]
        [InlineData(MTRFXXCommand.On)]
        public void TestWhenAdapterReceivedCommandWithUnknownChannel_ThenError(MTRFXXCommand command)
        {
            var configMock = new Mock<INooliteFSensorInfoRepository>();

            var adapterMock = new Mock<IMtrfAdapter>();

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, configMock.Object, Mock.Of<ILogger>());
            sensor.Activate();

            void Act() => adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(command, 13));

            Assert.Throws<InvalidConfigurationException>((Action)Act);
        }
    }
}
