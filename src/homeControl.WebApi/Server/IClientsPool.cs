namespace homeControl.WebApi.Server
{
    internal interface IClientsPool
    {
        void Add(IClientProcessor client);
        void Remove(IClientProcessor client);
    }
}