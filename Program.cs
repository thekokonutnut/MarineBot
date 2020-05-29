using System;
using System.Text;

namespace MarineBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var b = new Bot())
            {
                b.RunAsync().Wait();
            }
        }
    }
}
