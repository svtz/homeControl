using homeControl.ClientServerShared;

namespace homeControl.ClientApi.Server
{
    interface IClientRequestRouter
    {
        void Process(IClientMessage msg);
    }
}