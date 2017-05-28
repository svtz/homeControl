using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using homeControl.ClientServerShared;
using homeControl.ClientServerShared.Switches;
using Xunit;

namespace homeControl.ClientApi.Tests
{
    public class ApiCallerTests
    {
        private static readonly Guid _guid = Guid.NewGuid();
        private static double GetDouble() => 0.33;

        private static SwitchDto CreateRandomSwitch()
        {
            return new SwitchDto
            {
                Automation = SwitchAutomation.Supported,
                Description = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                Kind = SwitchKind.GradientSwitch,
                Name = Guid.NewGuid().ToString()
            };
        }

        public static readonly object[][] SwitchesApiCalls =
        {
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.DisableAutomation(_guid)), false
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.EnableAutomation(_guid)), true
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.GetDescriptions()), null
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.GetDescriptions()), new [] { CreateRandomSwitch(), CreateRandomSwitch() }
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, 32765)), true
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, GetDouble())), true
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, "string")), false
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, true)), false
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, Tuple.Create(323, false))), false
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.SetValue(_guid, null)), false
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.TurnOff(_guid)), true
            },
            new object[]
            {
                (Expression<Func<ISwitchesApiV1, object>>)(api => api.TurnOn(_guid)), true
            }
        };

        private sealed class ApiMock : ISwitchesApiV1
        {
            private readonly List<ApiRequest> _invocations = new List<ApiRequest>();
            public IReadOnlyCollection<ApiRequest> Calls { get; }
            public object ReturnValueForAllMethods { get; set; }

            public ApiMock()
            {
                Calls = new ReadOnlyCollection<ApiRequest>(_invocations);
            }

            public SwitchDto[] GetDescriptions()
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.GetDescriptions()));
                return (SwitchDto[])ReturnValueForAllMethods;
            }

            public bool SetValue(Guid id, object value)
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.SetValue(id, value)));
                return (bool)ReturnValueForAllMethods;
            }

            public bool TurnOn(Guid id)
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.TurnOn(id)));
                return (bool)ReturnValueForAllMethods;
            }

            public bool TurnOff(Guid id)
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.TurnOff(id)));
                return (bool)ReturnValueForAllMethods;
            }

            public bool EnableAutomation(Guid id)
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.EnableAutomation(id)));
                return (bool)ReturnValueForAllMethods;
            }

            public bool DisableAutomation(Guid id)
            {
                _invocations.Add(ApiRequest.Create<ISwitchesApiV1>(a => a.DisableAutomation(id)));
                return (bool)ReturnValueForAllMethods;
            }
        }

        [Theory]
        [MemberData(nameof(SwitchesApiCalls))]
        public void TestSwitchesApi(Expression<Func<ISwitchesApiV1, object>> apiCall, object expectedReturnValue)
        {
            var request = ApiRequest.Create(apiCall);
            var apiMock = new ApiMock
            {
                ReturnValueForAllMethods = expectedReturnValue
            };

            var caller = new ApiCaller<ISwitchesApiV1>(apiMock);
            var response = caller.Call(request);

            Assert.Equal(ResponseCode.Success, response.Code);
            Assert.Equal(expectedReturnValue, response.ReturnValue);
            Assert.Equal(1, apiMock.Calls.Count);
            var actualCall = apiMock.Calls.Single();
            Assert.Equal(request.MethodName, actualCall.MethodName);
            Assert.Equal(request.ApiName, actualCall.ApiName);
            Assert.Equal(request.Parameters.Length, actualCall.Parameters.Length);
            for (var idx = 0; idx < request.Parameters.Length; idx++)
            {
                var expectedParameter = request.Parameters[idx];
                var actualParameter = actualCall.Parameters[idx];

                Assert.Equal(expectedParameter.ParameterType, actualParameter.ParameterType);
                Assert.Equal(expectedParameter.ParameterValue, actualParameter.ParameterValue);
            }
        }
    }
}
