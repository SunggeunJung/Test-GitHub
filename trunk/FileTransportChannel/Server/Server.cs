
namespace DuplexFileTransportChannelSample
{
    # region using

    using System;
    using System.ServiceModel;

    # endregion

    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("################    SERVER    ################");
            Console.ForegroundColor = ConsoleColor.Yellow;

            using (ServiceHost host = new ServiceHost
                (typeof(DuplexFileTransportChannelSample.ReverseStringService)))
            {
                host.Open();
                Console.WriteLine("The service is ready.");
                Console.ReadKey();
            }
        }
   }
}