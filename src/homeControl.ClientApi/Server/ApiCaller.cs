using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using homeControl.ClientServerShared;

namespace homeControl.ClientApi.Server
{
    internal sealed class ApiCaller<TApi> where TApi: class
    {
        private readonly TApi _api;

        public ApiCaller(TApi api)
        {
            Guard.DebugAssertArgumentNotNull(api, nameof(api));

            _api = api;
        }

        public ApiResponse Call(ApiRequest request)
        {
            Guard.DebugAssertArgumentNotNull(request, nameof(request));

            try
            {
                return CallImpl(request);
            }
            catch (Exception ex)
            {
                return new ApiResponse(request.RequestId, ResponseCode.Failure, ex.ToString());
            }
        }

        private static readonly MethodInfo[] _apiMethods
            = typeof(TApi)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .ToArray();

        private ApiResponse CallImpl(ApiRequest request)
        {
            Guard.DebugAssertArgumentNotNull(request, nameof(request));

            if (request.ApiName != typeof(TApi).FullName)
            {
                throw new NotSupportedException("Requested API is not supported.");
            }

            var method = _apiMethods
                .Where(m => m.Name == request.MethodName)
                .Where(m => ParametersMatch(m, request.Parameters));
        }

        private static bool ParametersMatch(MethodInfo method, ApiRequestParameter[] requestParameters)
        {
            Guard.DebugAssertArgumentNotNull(method, nameof(method));
            Guard.DebugAssertArgumentNotNull(requestParameters, nameof(requestParameters));

            var methodParameters = method.GetParameters();
            if (methodParameters.Length != requestParameters.Length)
                return false;

            var notMatched = methodParameters
                .PairWith(requestParameters)
                .FirstOrDefault(p => )

        }
    }

    internal static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<T1, T2>> PairWith<T1, T2>(this IEnumerable<T1> source, IEnumerable<T2> other)
        {
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(other, nameof(other));

            using (var e1 = source.GetEnumerator())
            using (var e2 = other.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    yield return Tuple.Create(e1.Current, e2.Current);
                }
            }
        }
    }
}
