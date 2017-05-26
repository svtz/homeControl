namespace homeControl.ClientServerShared
{
    public class ApiRequestParameter
    {
        public string ParameterType { get; }
        public object ParameterValue { get; }

        public ApiRequestParameter(string parameterType, object parameterValue)
        {
            ParameterType = parameterType;
            ParameterValue = parameterValue;
        }
    }
}