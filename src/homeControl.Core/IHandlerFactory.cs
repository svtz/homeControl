namespace homeControl.Core
{
    public interface IHandlerFactory
    {
        IHandler[] GetHandlers();
    }
}