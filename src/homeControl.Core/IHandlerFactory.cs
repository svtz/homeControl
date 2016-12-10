namespace homeControl.Core
{
    public interface IHandlerFactory
    {
        IHandler[] CreateHandlers();
    }
}