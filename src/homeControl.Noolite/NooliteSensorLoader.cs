using homeControl.Configuration;

namespace homeControl.Noolite
{
    internal sealed class NooliteSensorLoader : IInitializer
    {
        private readonly NooliteSensor _nooliteSensor;

        public NooliteSensorLoader(NooliteSensor nooliteSensor)
        {
            _nooliteSensor = nooliteSensor;
        }

        public void Init()
        {
            _nooliteSensor.Activate();
        }
    }
}
