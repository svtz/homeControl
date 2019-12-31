using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using homeControl.Configuration;
using homeControl.Domain;
using homeControl.Domain.Events;
using homeControl.Domain.Events.Sensors;
using homeControl.Domain.Events.Switches;
using homeControl.NooliteF;
using homeControl.NooliteF.Adapters;
using homeControl.NooliteF.Configuration;
using homeControl.NooliteF.SwitchController;
using Moq;
using Serilog;
using ThinkingHome.NooLite;
using ThinkingHome.NooLite.Internal;
using Xunit;

namespace homeControl.Tests.NooliteF
{
    public class NooliteFSensorTests
    {
        private ReceivedData CreateCommandData(MTRFXXMode mode, MTRFXXCommand cmd, byte channel, byte[] data = null)
        {
            var buf = new byte[17];
            buf[0] = 173;
            buf[1] = (byte)mode;
            buf[16] = 174; 
            buf[4] = channel;
            buf[5] = (byte)cmd;

            if (data != null)
            {
                if (data.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(data));
                Array.Copy(data, 0, buf, 7, 4);
                return new MicroclimateData(buf);
            }

            return new ReceivedData(buf);
        }

        private INooliteFSwitchInfoRepository GetEmptySwitchesRepository()
        {
            var mock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            mock.Setup(m => m.GetAll())
                .Returns(Task.FromResult<IReadOnlyCollection<NooliteFSwitchInfo>>(new NooliteFSwitchInfo[0]));

            return mock.Object;
        }
        
        private INooliteFSwitchInfoRepository GetConfiguredSwitchesRepository(params NooliteFSwitchInfo[] configs)
        {
            var mock = new Mock<INooliteFSwitchInfoRepository>(MockBehavior.Strict);
            mock.Setup(m => m.GetAll())
                .Returns(Task.FromResult<IReadOnlyCollection<NooliteFSwitchInfo>>(configs));

            mock.Setup(m => m.ContainsConfig(It.IsAny<SwitchId>())).Returns(Task.FromResult(false));
            
            foreach (var config in configs)
            {
                mock.Setup(m => m.GetConfig(config.SwitchId)).Returns(Task.FromResult(config));
                mock.Setup(m => m.ContainsConfig(config.SwitchId)).Returns(Task.FromResult(true));
            }
            
            return mock.Object;
        }
        
        private INooliteFSensorInfoRepository GetEmptySensorsRepository()
        {
            var mock = new Mock<INooliteFSensorInfoRepository>(MockBehavior.Strict);
            mock.Setup(m => m.GetAll())
                .Returns(Task.FromResult<IReadOnlyCollection<AbstractNooliteFSensorInfo>>(new AbstractNooliteFSensorInfo[0]));

            return mock.Object;
        }
        
        private INooliteFSensorInfoRepository GetConfiguredSensorsRepository(params AbstractNooliteFSensorInfo[] configs)
        {
            var mock = new Mock<INooliteFSensorInfoRepository>(MockBehavior.Strict);
            mock.Setup(m => m.GetAll())
                .Returns(Task.FromResult<IReadOnlyCollection<AbstractNooliteFSensorInfo>>(configs));
            
            return mock.Object;
        }
        
        [Theory]
        [InlineData(MTRFXXCommand.On, 1, 0)]
        [InlineData(MTRFXXCommand.Off, 0, 1)]
        public void TestWhenAdapterReceivedCommand_ThenRaiseEvent(MTRFXXCommand command, int expectedActivateCallCount, int expectedDeactivateCallCount)
        {
            var sensorConfig = new OnOffNooliteFSensorInfo {SensorId = SensorId.NewId(), Channel = 17 };
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));
            gateMock.Setup(g => g.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                GetEmptySwitchesRepository(), new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX, command, sensorConfig.Channel));

            gateMock.Verify(m => m.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedActivateCallCount));
            gateMock.Verify(m => m.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Exactly(expectedDeactivateCallCount));
        }

        [Fact]
        public void TestWhenReceivedMicroclimate_ThenSendTemperature()
        {
            var sensorConfig = new TemperatureNooliteFSensorInfo {TemperatureSensorId = SensorId.NewId(), Channel = 12 };
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);
            const decimal temperature = 25.1m; 
            var data = new byte[] {251, 32, 0, 0};
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.Is<SensorValueEvent>(e => e.SensorId == sensorConfig.TemperatureSensorId && e.Value == temperature)));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                GetEmptySwitchesRepository(), new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.MicroclimateData, sensorConfig.Channel, data));

            gateMock.Verify(m => m.SendEvent(It.Is<SensorValueEvent>(e => e.SensorId == sensorConfig.TemperatureSensorId && e.Value == temperature)), Times.Once);
        }
        
        [Fact]
        public void TestWhenReceivedMicroclimateWithHumidity_ThenSendTemperatureAndHumidity()
        {
            var sensorConfig = new TemperatureAndHumidityNooliteFSensorInfo {TemperatureSensorId = SensorId.NewId(), HumiditySensorId = SensorId.NewId(), Channel = 12 };
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);
            const decimal temperature = 25.2m; 
            const decimal humidity = 38; 
            var data = new byte[] {252, 32, 38, 255};
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<SensorValueEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                GetEmptySwitchesRepository(), new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.MicroclimateData, sensorConfig.Channel, data));

            gateMock.Verify(m => m.SendEvent(It.Is<SensorValueEvent>(e => e.SensorId == sensorConfig.TemperatureSensorId && e.Value == temperature)), Times.Once);
            gateMock.Verify(m => m.SendEvent(It.Is<SensorValueEvent>(e => e.SensorId == sensorConfig.HumiditySensorId && e.Value == humidity)), Times.Once);
        }
        
        [Theory]
        [InlineData(MTRFXXCommand.Off)]
        [InlineData(MTRFXXCommand.On)]
        public void TestWhenAdapterReceivedCommandWithUnknownChannel_ThenError(MTRFXXCommand command)
        {
            var sensorConfigRepo = GetEmptySensorsRepository();

            var adapterMock = new Mock<IMtrfAdapter>();

            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                GetEmptySwitchesRepository(), new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            void Act() => adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX, command, 13));

            Assert.Throws<InvalidConfigurationException>(Act);
        }
        
        [Fact]
        public void TestWhenStatusChannelsNull_ThenDoesNotThrow()
        {
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, StatusChannelIds = null, SwitchId = SwitchId.NewId() };
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);

            var sensorConfig = new OnOffNooliteFSensorInfo { Channel = 13, SensorId = SensorId.NewId() };
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<SensorActivatedEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                switchConfigRepo, new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.On, sensorConfig.Channel)); 
        }
        
        [Fact]
        public void TestWhenReceivedRxOnCommand_ThenSendOnEventAndChangeStatus()
        {
            var statusChannels = new byte[] {10, 13};
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, StatusChannelIds = statusChannels, SwitchId = SwitchId.NewId() };
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOnEvent>()));
            
            var statusHolder = new NooliteFSwitchesStatusHolder();
            var adapterMock = new Mock<IMtrfAdapter>();

            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, statusHolder, Mock.Of<ILogger>());
            sensor.Activate();

            statusHolder.SetStatus(switchConfig.Channel, 0, false);
            
            foreach (var statusChannel in statusChannels)
            {
                adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.On, statusChannel));
                gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOnEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
                gateMock.Invocations.Clear();
            }

            var status = statusHolder.GetStatus(switchConfig.Channel);
            Assert.True(status.HasValue && status.Value.isOn);
        }
        
        [Fact]
        public void TestWhenReceivedRxOffCommand_ThenSendOffEventAndChangeStatus()
        {
            var statusChannels = new byte[] {10, 13};
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, StatusChannelIds = statusChannels, SwitchId = SwitchId.NewId() };
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOffEvent>()));

            var statusHolder = new NooliteFSwitchesStatusHolder();
            statusHolder.SetStatus(switchConfig.Channel, 1, true);

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, statusHolder, Mock.Of<ILogger>());
            sensor.Activate();

            foreach (var statusChannel in statusChannels)
            {
                adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.Off, statusChannel));
                gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOffEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
                gateMock.Invocations.Clear();
            }
            
            var status = statusHolder.GetStatus(switchConfig.Channel);
            Assert.True(status.HasValue && !status.Value.isOn);
        }
        
        [Fact]
        public void TestWhenConfiguredBothSwitchStateChannelAndSensorAndReceivedOn_ThenSendBothEvents()
        {
            const byte channel = 10;
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, StatusChannelIds = new [] {channel}, SwitchId = SwitchId.NewId() };
            var sensorConfig = new OnOffNooliteFSensorInfo { Channel = channel, SensorId = SensorId.NewId() };
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOnEvent>()));
            gateMock.Setup(g => g.SendEvent(It.IsAny<SensorActivatedEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                switchConfigRepo, new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.On, channel));
            
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOnEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
            gateMock.Verify(m => m.SendEvent(It.Is<SensorActivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Once);
        }

        [Fact]
        public void TestWhenConfiguredBothSwitchStateChannelAndSensorAndReceivedOff_ThenSendBothEvents()
        {
            const byte channel = 10;
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, StatusChannelIds = new [] {channel}, SwitchId = SwitchId.NewId() };
            var sensorConfig = new OnOffNooliteFSensorInfo { Channel = channel, SensorId = SensorId.NewId() };
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            var sensorConfigRepo = GetConfiguredSensorsRepository(sensorConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOffEvent>()));
            gateMock.Setup(g => g.SendEvent(It.IsAny<SensorDeactivatedEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, sensorConfigRepo,
                switchConfigRepo, new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.RX,MTRFXXCommand.Off, channel));
            
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOffEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
            gateMock.Verify(m => m.SendEvent(It.Is<SensorDeactivatedEvent>(e => e.SensorId == sensorConfig.SensorId)), Times.Once);
        }

        [Fact]
        public void TestWhenReceivedTxOnCallback_ThenSendOnEventAndSetStatus()
        {
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, SwitchId = SwitchId.NewId(), UseF = false};
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOnEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.TX,MTRFXXCommand.On, switchConfig.Channel));
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOnEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
        }
        
        [Fact]
        public void TestWhenReceivedTxOffCallback_ThenSendOffEventAndSetStatus()
        {
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, SwitchId = SwitchId.NewId(), UseF = false};
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOffEvent>()));

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, new NooliteFSwitchesStatusHolder(), Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, CreateCommandData(MTRFXXMode.TX,MTRFXXCommand.Off, switchConfig.Channel));
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOffEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
        }

        [Fact]
        public void TestWhenReceivedSendStateWithOn_ThenSendOnEvent()
        {
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, SwitchId = SwitchId.NewId(), UseF = true};
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOnEvent>()));
            
            var statusHolder = new NooliteFSwitchesStatusHolder();
            statusHolder.SetStatus(switchConfig.Channel, 0, false);

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, statusHolder, Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, 
                CreateCommandData(MTRFXXMode.TXF,MTRFXXCommand.SendState, switchConfig.Channel, new byte[] {0, 0, 1, 0}));
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOnEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
            
            var status = statusHolder.GetStatus(switchConfig.Channel);
            Assert.True(status.HasValue && status.Value.isOn);
        }
        
        [Fact]
        public void TestWhenReceivedSendStateWithOff_ThenSendOffEvent()
        {
            var switchConfig = new NooliteFSwitchInfo { Channel = 15, SwitchId = SwitchId.NewId(), UseF = true};
            var switchConfigRepo = GetConfiguredSwitchesRepository(switchConfig);
            
            var gateMock = new Mock<IEventSender>(MockBehavior.Strict);
            gateMock.Setup(g => g.SendEvent(It.IsAny<TurnSwitchOffEvent>()));
            
            var statusHolder = new NooliteFSwitchesStatusHolder();
            statusHolder.SetStatus(switchConfig.Channel, 1, true);

            var adapterMock = new Mock<IMtrfAdapter>();
            var sensor = new NooliteFSensor(gateMock.Object, adapterMock.Object, GetEmptySensorsRepository(),
                switchConfigRepo, statusHolder, Mock.Of<ILogger>());
            sensor.Activate();

            adapterMock.Raise(ad => ad.ReceiveData += null, null, 
                CreateCommandData(MTRFXXMode.TXF,MTRFXXCommand.SendState, switchConfig.Channel, new byte[] {0, 0, 0, 0}));
            gateMock.Verify(m => m.SendEvent(It.Is<TurnSwitchOffEvent>(e => e.SwitchId == switchConfig.SwitchId)), Times.Once);
            
            var status = statusHolder.GetStatus(switchConfig.Channel);
            Assert.True(status.HasValue && !status.Value.isOn);
        }
    }
}
