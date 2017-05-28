using System;
using System.Linq.Expressions;
using homeControl.ClientServerShared.Switches;
using Xunit;

namespace homeControl.ClientServerShared.Tests
{
    public class ApiRequestTest
    {
        private static readonly Guid _guid = Guid.NewGuid();
        private static double GetDouble() => 0.33;

        public static readonly object[][] SwitchesApiCalls =
        {
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.DisableAutomation(_guid)),
                nameof(ISwitchesApiV1.DisableAutomation),
                new[] {typeof(Guid)},
                new object[] {_guid}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.EnableAutomation(_guid)),
                nameof(ISwitchesApiV1.EnableAutomation),
                new[] {typeof(Guid)},
                new object[] {_guid}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.GetDescriptions()),
                nameof(ISwitchesApiV1.GetDescriptions),
                new Type[0],
                new object[0]
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, 32765)),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, 32765}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, GetDouble())),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, GetDouble()}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, "string")),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, "string"}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, true)),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, true}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, Tuple.Create(323, false))),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, Tuple.Create(323, false)}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, null)),
                nameof(ISwitchesApiV1.SetValue),
                new[] {typeof(Guid), typeof(object)},
                new object[] {_guid, null}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.TurnOff(_guid)),
                nameof(ISwitchesApiV1.TurnOff),
                new[] {typeof(Guid)},
                new object[] {_guid}
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.TurnOn(_guid)),
                nameof(ISwitchesApiV1.TurnOn),
                new[] {typeof(Guid)},
                new object[] {_guid},
            }
        };

        [Theory]
        [MemberData(nameof(SwitchesApiCalls))]
        public void TestSwitchesApiCalls(Expression<Func<ISwitchesApiV1, object>> call, 
            string expectedMethod, Type[] expectedTypes, object[] expectedParameters)
        {
            Assert.Equal(expectedTypes.Length, expectedParameters.Length);

            var request = ApiRequest.Create(call);

            Assert.Equal(expectedMethod, request.MethodName);
            Assert.NotEqual(Guid.Empty, request.RequestId);
            Assert.Equal(typeof(ISwitchesApiV1).FullName, request.ApiName);
            Assert.Equal(expectedParameters.Length, request.Parameters.Length);

            for (var idx = 0; idx < expectedParameters.Length; idx++)
            {
                var expectedValue = expectedParameters[idx];
                var expectedType = expectedTypes[idx];

                var actualParameter = request.Parameters[idx];

                Assert.Equal(expectedType.FullName, actualParameter.ParameterType);
                Assert.Equal(expectedValue, actualParameter.ParameterValue);
            }
        }
    }
}
