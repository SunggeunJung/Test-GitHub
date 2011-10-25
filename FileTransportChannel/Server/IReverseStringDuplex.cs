using System;
using System.ServiceModel;

namespace DuplexFileTransportChannelSample
{
    [ServiceContract(CallbackContract = typeof(IReverseStringDuplexCallback))]
    public interface IReverseStringDuplex
    {
        [OperationContract(IsOneWay = true)]
        void ReverseString(string originalString);
    }

    public interface IReverseStringDuplexCallback
    {
        [OperationContract(IsOneWay=true)]
        void PrintResult(string reversedString); 
    }
}
