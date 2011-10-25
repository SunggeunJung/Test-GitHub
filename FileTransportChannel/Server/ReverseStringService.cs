
namespace DuplexFileTransportChannelSample
{
    # region using

    using System;
    using System.Text;
    using System.ServiceModel;

    # endregion

    public class ReverseStringService : IReverseStringDuplex 
    {
        IReverseStringDuplexCallback CallBack
        {
            get 
            {
                return OperationContext.Current.GetCallbackChannel<IReverseStringDuplexCallback>(); 
            }
        }

        void IReverseStringDuplex.ReverseString(string inputString)
        {
            Console.WriteLine("Received input string : {0}", inputString);
            char[] inputStringArray = inputString.ToCharArray();
            
            char temp;
            for (int loop = 0; loop <= (inputStringArray.Length -1) / 2; loop++)
            {
                temp = inputStringArray[loop];
                inputStringArray[loop] = inputStringArray[inputStringArray.Length - loop - 1];
                inputStringArray[inputStringArray.Length - loop - 1] = temp;
            }

            string outputString;
            outputString = new String(inputStringArray);
            Console.WriteLine("Sending reversed string : {0}", outputString);
            CallBack.PrintResult(outputString);
        }
    }
}