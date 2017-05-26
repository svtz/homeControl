using System;

namespace homeControl.ClientServerShared
{
    public sealed class ApiResponse : AbstractMessage
    {
        public ResponseCode Code;

        public object ReturnValue { get; }

        public ApiResponse(Guid id, ResponseCode code, object returnValue) : base(id)
        {
            ReturnValue = returnValue;
            Code = code;
        }
    }
}