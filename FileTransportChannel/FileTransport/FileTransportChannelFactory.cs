
namespace FileTransport
{
    # region using

    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    # endregion

    class FileTransportChannelFactory : ChannelFactoryBase<IDuplexChannel>
    {
        # region member_variables

        private bool streamed;
        private string scheme;
        private BufferManager bufferManager;
        private MessageEncoderFactory messageEncoderFactory;
        public readonly long MaxReceivedMessageSize;

        # endregion

        # region Properties

        public bool Streamed
        {
            get { return streamed; }
        }

        public string Scheme
        {
            get { return scheme; }
        }

        public BufferManager BufferManager
        {
            get { return bufferManager; }
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get { return MessageEncoderFactory; }
        }

        public MessageVersion MessageVersion
        {
            get { return MessageVersion.Default; }
        }

        # endregion

        # region Constructor

        public FileTransportChannelFactory(FileTransportBindingElement transportElement,
            BindingContext context) : base(context.Binding)
        {
            this.streamed = transportElement.Streamed;
            this.scheme = transportElement.Scheme;
            this.bufferManager = BufferManager.CreateBufferManager
                (transportElement.MaxBufferPoolSize, int.MaxValue);
            MessageEncodingBindingElement messageEncodingElement =
                context.BindingParameters.Remove<MessageEncodingBindingElement>();
            if (messageEncodingElement != null)
            {
                this.messageEncoderFactory =
                    messageEncodingElement.CreateMessageEncoderFactory();
            }
            else
            {
                this.messageEncoderFactory =
                    FileTransportChannelUtils.DefaultMessageEncoderFactory;
            }
            this.MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
        }

        # endregion

        # region CreateChannel event

        protected override IDuplexChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new FileTransportChannel
                (this.bufferManager, this.messageEncoderFactory, address, this, via);
        }

        # endregion

        # region Open events

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        # endregion
    }
}
