namespace DuplexFileTransportChannelSample
{
    # region using

    using System;
    using System.IO;
    using System.ServiceModel;

    # endregion

    class Client
    {
        private const string ReverseStringFolderName = "ReverseStringFolder";
        static void Main(string[] args)
        {
            Console.WriteLine("################    CLIENT    ################");
            Console.ForegroundColor = ConsoleColor.Green;

            // Construct InstanceContext to handle messages on callback interface
            InstanceContext instanceContext = new InstanceContext(new CallBackHandler());
            ReverseStringDuplexClient client = new ReverseStringDuplexClient(instanceContext);

            // Create a client
            string inputString;

            // Delete the FileTransport Folder if present from an earlier run
            if (Directory.Exists("C:\\" + ReverseStringFolderName))
            {
                Directory.Delete("C:\\" + ReverseStringFolderName, true);
            }

            Console.WriteLine("Press Enter when the Serivce is ready");
            Console.ReadKey();

            // Get the string from the user
            Console.Write("Enter the string you want to reverse : \t");
            inputString = Console.ReadLine();
            Console.WriteLine("Calling WCF service with input as : {0}", inputString);
            client.ReverseString(inputString);

            Console.ReadKey();
        }

        public class CallBackHandler : IReverseStringDuplexCallback
        {
            public void PrintResult(string reversedString)
            {
                Console.WriteLine("Received reversed string : {0}", reversedString);
            }
        }
    }
}
