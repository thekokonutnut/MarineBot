using System;
using System.Text;
using System.Threading;

namespace MarineBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            bool allowrestart = true;

            while (allowrestart)
            {
                using (var b = new Bot())
                {
                    b.RunAsync().Wait();
                    allowrestart = !b.HandledExit;
                }

                Console.WriteLine("[System] Bot loop exited.");

                if (allowrestart)
                {
                    Console.WriteLine("[System] Restarting in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
