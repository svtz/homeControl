using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace homeControl.ClientServerShared
{
    public sealed class ApiRequest : AbstractMessage
    {
        public string ApiName { get; }
        public string MethodName { get; }
        public ApiRequestParameter[] Parameters { get; set; }

        private ApiRequest(
            string apiName, string methodName, 
            ApiRequestParameter[] parameters)
            : base(Guid.NewGuid())
        {
            ApiName = apiName;
            MethodName = methodName;
            Parameters = parameters;
        }

        public void Create<TApi>(Expression<Func<TApi, object>> apiCall)
        {
            if (apiCall == null)
                throw new ArgumentNullException(nameof(apiCall));

            var callExpression = apiCall.Body as MethodCallExpression;
            if (callExpression == null)
                throw new ArgumentOutOfRangeException(nameof(apiCall));

            var parameters = new List<ApiRequestParameter>(callExpression.Arguments.Count);
            foreach (var arg in callExpression.Arguments)
            {
                var parameterType = arg.Type;
                var parameterValue = Expression.Lambda<object>(arg).Compile();
            }
        }
    }
}