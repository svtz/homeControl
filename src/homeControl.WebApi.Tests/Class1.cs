using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeControl.Core;
using homeControl.WebApi.Controllers;
using Moq;
using Xunit;

namespace homeControl.WebApi.Tests
{
    public class SwitchesControllerTests
    {
        [Fact]
        public void TestGetSwitches_ReturnsConfiguredSwitches()
        {
            var controller = new SwitchesController(Mock.Of<IEventPublisher>());

            var switches = controller.GetAll();
        }
    }
}
