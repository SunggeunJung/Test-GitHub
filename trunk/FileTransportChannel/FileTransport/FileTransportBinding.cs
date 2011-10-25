
namespace FileTransport
{
    # region using

    using System.ServiceModel.Channels;
    using Microsoft.ServiceModel.Samples;
    
    # endregion

    public class FileTransportBinding : Binding 
    {
        readonly GZipMessageEncodingBindingElement messageEncodingBindingElement;
        readonly FileTransportBindingElement transportBindingElement;
        
        public FileTransportBinding ()
        {
            this.messageEncodingBindingElement = new GZipMessageEncodingBindingElement();
            this.transportBindingElement = new FileTransportBindingElement();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElementsCollection = new BindingElementCollection();
            bindingElementsCollection.Add(this.messageEncodingBindingElement);
            bindingElementsCollection.Add(this.transportBindingElement);
            return bindingElementsCollection.Clone();
        }

        public override string Scheme
        {
            get { return transportBindingElement.Scheme; }
        }
    }
}
