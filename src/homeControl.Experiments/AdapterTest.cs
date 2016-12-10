using System;
using System.Threading;
using ThinkingHome.NooLite;

namespace homeControl.Experiments
{
    class AdapterTest
    {
        public void Run()
        {
            var ad = new RX2164Adapter();
            if (!ad.OpenDevice())
                throw new Exception("unable to open");

            ad.CommandReceived += Ad_CommandReceived;

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static void Ad_CommandReceived(ThinkingHome.NooLite.ReceivedData.ReceivedCommandData obj)
        {
            Console.WriteLine($"Received command: {obj.Cmd}");
        }
    }
}