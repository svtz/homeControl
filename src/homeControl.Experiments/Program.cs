using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThinkingHome.NooLite;

namespace homeControl.Experiments
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ad = new RX2164Adapter();
            if (!ad.OpenDevice())
                throw new Exception("unable to open");

            ad.CommandReceived += Ad_CommandReceived;

            while (true)
            {
                Thread.Sleep(100);
            }

            ad.Dispose();
        }

        private static void Ad_CommandReceived(ThinkingHome.NooLite.ReceivedData.ReceivedCommandData obj)
        {
            
        }
    }
}
