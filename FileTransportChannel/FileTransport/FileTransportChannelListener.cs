
namespace FileTransport
{
    # region using

    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    
    # endregion

    class FileTransportChannelListener : ChannelListenerBase<IDuplexChannel>
    {
        # region member_variables
        
        private bool streamed;
        private string scheme;
        private Uri uri;
        private BufferManager bufferManager;
        private MessageEncoderFactory messageEncoderFactory;
        public readonly long MaxReceivedMessageSize;

        private delegate bool WaitForRequestDelegate
            (TimeSpan timeout, string absolutePath, string fileName);
        private delegate bool WaitForDirectoryDelegate
            (TimeSpan timeout, string fileName);

        //private delegate bool CreateDirectoryIfNotPresentDelegate(string absolutePath);

        WaitForRequestDelegate waitForRequestDelegate;
        WaitForDirectoryDelegate waitForDirectoryDelegate;

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

        public override Uri Uri
        {
            get { return uri; }
        }

        public MessageVersion MessageVersion
        {
            get { return MessageVersion.Default; }
        }

        public BufferManager BufferManager
        {
            get { return bufferManager; }
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get { return messageEncoderFactory; }
        }

        # endregion

        # region Constructor

        internal FileTransportChannelListener(FileTransportBindingElement transportElement,
            BindingContext context) : base(context.Binding)
        {
            this.streamed = transportElement.Streamed;
            this.scheme = transportElement.Scheme;
            this.uri = new Uri(context.ListenUriBaseAddress, 
                context.ListenUriRelativeAddress);
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
            waitForRequestDelegate = new WaitForRequestDelegate
                (FileTransportChannelUtils.WaitForFileMessage);
            waitForDirectoryDelegate = new WaitForDirectoryDelegate
                (FileTransportChannelUtils.WaitForDirectoryChannel);
        }

        # endregion

        # region WaitForChannel events

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        
        # endregion

        # region AcceptChannel events
        
        protected override IDuplexChannel OnAcceptChannel(TimeSpan timeout)
        {
            EndpointAddress address = new EndpointAddress(this.uri);
            return new FileTransportChannel
                (this.bufferManager, this.messageEncoderFactory, address, this);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return waitForDirectoryDelegate.BeginInvoke(timeout,                        
                FileTransportChannelUtils.ReverseStringFolderName, callback, state);
        }

        protected override IDuplexChannel OnEndAcceptChannel(IAsyncResult result)
        {
            EndpointAddress address = new EndpointAddress(this.uri);
            return new FileTransportChannel
                (this.bufferManager, this.messageEncoderFactory, address, this);
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

        # region Abort event

        protected override void OnAbort()
        {
            throw new NotImplementedException();
        }

        # endregion

        # region Close events

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        # endregion
    }
}

