namespace homeControl.ClientApi.Server
{
    internal interface IClientsPool
    {
        void Add(IClientProcessor client);
        void Remove(IClientProcessor client);
    }
}