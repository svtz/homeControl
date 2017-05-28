using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace homeControl.ClientServerShared
{
    public sealed class ApiRequest : AbstractMessage
    {
        public string ApiName { get; }
        public string MethodName { get; }
        public ApiRequestParameter[] Parameters { get; }

        private ApiRequest(
            string apiName, string methodName, 
            ApiRequestParameter[] parameters)
            : base(Guid.NewGuid())
        {
            if (apiName == null) throw new ArgumentNullException(nameof(apiName));
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            ApiName = apiName;
            MethodName = methodName;
            Parameters = parameters;
        }

        public static ApiRequest Create<TApi>(Expression<Func<TApi, object>> apiCall)
        {
            if (apiCall == null) throw new ArgumentNullException(nameof(apiCall));
            
            var convertExpression = apiCall.Body as UnaryExpression;
            var callExpression = (convertExpression?.Operand ?? apiCall.Body) as MethodCallExpression;
            if (callExpression == null)
                throw new ArgumentOutOfRangeException(nameof(apiCall));

            var methodArgs = callExpression.Method.GetParameters();
            Guard.DebugAssert(methodArgs.Length == callExpression.Arguments.Count, "methodArgs.Length == callExpression.Arguments.Count");

            var parameters = new List<ApiRequestParameter>(callExpression.Arguments.Count);
            for (var idx = 0; idx < callExpression.Arguments.Count; idx++)
            {
                var expressionArg = callExpression.Arguments[idx];
                var methodArg = methodArgs[idx];

                var parameterType = methodArg.ParameterType;
                var cast = Expression.Convert(expressionArg, typeof(object));
                var valueAccessor = Expression.Lambda<Func<object>>(cast).Compile();

                parameters.Add(new ApiRequestParameter(TypeSerializer.Serialize(parameterType), valueAccessor()));
            }

            return new ApiRequest(TypeSerializer.Serialize<TApi>(), callExpression.Method.Name, parameters.ToArray());
        }
    }
}