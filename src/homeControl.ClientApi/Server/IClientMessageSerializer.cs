using homeControl.ClientServerShared;

namespace homeControl.ClientApi.Server
{
    internal interface IClientMessageSerializer
    {
        byte[] Serialize(IClientMessage message);
    }
}