using homeControl.Configuration.Switches;
using Moq;
using Xunit;

namespace homeControl.Peripherals.Tests
{
    public class SwitchControllerSelectorTests
    {
        [Fact]
        public void TestCanHandle_WhenImplementationCanHandle()
        {
            var switchId = SwitchId.NewId();
            var implCanHandleMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implCanHandleMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            var implCannotHandleMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implCannotHandleMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(false);

            var selector = new SwitchControllerSelector(new [] { implCanHandleMock.Object, implCannotHandleMock.Object });

            Assert.True(selector.CanHandleSwitch(switchId));
        }

        [Fact]
        public void TestCantHandle_WhenImplementationCantHandle()
        {
            var switchId = SwitchId.NewId();
            var impl1Mock = new Mock<ISwitchController>(MockBehavior.Strict);
            impl1Mock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(false);
            var impl2Mock = new Mock<ISwitchController>(MockBehavior.Strict);
            impl2Mock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(false);

            var selector = new SwitchControllerSelector(new[] { impl1Mock.Object, impl2Mock.Object });

            Assert.False(selector.CanHandleSwitch(switchId));
        }

        [Fact]
        public void Test_SelectorCallsAllSuitableImplementations()
        {
            var switchId = SwitchId.NewId();
            var implCannotHandleMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implCannotHandleMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(false);
            var implCanHandle1Mock = new Mock<ISwitchController>(MockBehavior.Strict);
            implCanHandle1Mock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            implCanHandle1Mock.Setup(cntr => cntr.TurnOn(switchId));
            var implCanHandle2Mock = new Mock<ISwitchController>(MockBehavior.Strict);
            implCanHandle2Mock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            implCanHandle2Mock.Setup(cntr => cntr.TurnOn(switchId));

            var selector = new SwitchControllerSelector(new[] { implCannotHandleMock.Object, implCanHandle1Mock.Object, implCanHandle2Mock.Object });
            selector.TurnOn(switchId);
            
            implCanHandle1Mock.Verify(cntr => cntr.TurnOn(switchId), Times.Once);
            implCanHandle2Mock.Verify(cntr => cntr.TurnOn(switchId), Times.Once);
            implCannotHandleMock.Verify(cntr => cntr.TurnOn(switchId), Times.Never);
        }

        [Fact]
        public void Test_SelectorTurnOn_CallsImplsTurnOn()
        {
            var switchId = SwitchId.NewId();
            var implMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            implMock.Setup(cntr => cntr.TurnOn(switchId));

            var selector = new SwitchControllerSelector(new[] { implMock.Object });
            selector.TurnOn(switchId);

            implMock.Verify(cntr => cntr.TurnOn(switchId), Times.Once);
        }

        [Fact]
        public void Test_SelectorTurnOff_CallsImplsTurnOff()
        {
            var switchId = SwitchId.NewId();
            var implMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            implMock.Setup(cntr => cntr.TurnOff(switchId));

            var selector = new SwitchControllerSelector(new[] { implMock.Object });
            selector.TurnOff(switchId);

            implMock.Verify(cntr => cntr.TurnOff(switchId), Times.Once);
        }

        [Fact]
        public void Test_SelectorSetPower_CallsImplsSetPower()
        {
            var switchId = SwitchId.NewId();
            const double power = 0.73;
            var implMock = new Mock<ISwitchController>(MockBehavior.Strict);
            implMock.Setup(cntr => cntr.CanHandleSwitch(switchId)).Returns(true);
            implMock.Setup(cntr => cntr.SetPower(switchId, power));

            var selector = new SwitchControllerSelector(new[] { implMock.Object });
            selector.SetPower(switchId, power);

            implMock.Verify(cntr => cntr.SetPower(switchId, power), Times.Once);
        }
    }
}
