namespace FileTransport
{
    # region using

    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    
    # endregion

    class FileTransportChannel : IDuplexChannel
    {
        # region member_variables
        
        private Uri via;
        private EndpointAddress localAddress;
        private Uri path;
        private BufferManager bufferManager;
        private MessageEncoder messageEncoder;
        private EndpointAddress remoteAddress;
        private bool streamed;
        readonly long maxReceivedMessageSize;
        private CommunicationState state = CommunicationState.Closed;
        private Message clientMessage;

        private string writeFileName;
        private string readFileName;

        private bool clientFileRead;
        private bool serverFileRead;
        
        private delegate bool WaitForFileMessageDelegate
            (TimeSpan timeout, string absolutePath, string fileName);

        WaitForFileMessageDelegate waitForFileMessageDelegate 
            = new WaitForFileMessageDelegate(FileTransportChannelUtils.WaitForFileMessage);

        private delegate Message GetMessageDelegate();

        # endregion

        # region Properties

        public Uri Via
        {
            get { return via;  }
        }

        public EndpointAddress LocalAddress
        {
            get { return localAddress; }
        }

        public Uri Path
        {
            get { return path; }
        }

        public BufferManager BufferManager
        {
            get { return bufferManager; }
        }

        public MessageEncoder MessageEncoder
        {
            get { return messageEncoder; }
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.remoteAddress; }
        }

        public bool Streamed
        {
            get { return streamed; }
        }

        # endregion

        # region Constructors

        public FileTransportChannel(BufferManager bufferManager,
            MessageEncoderFactory messageEncoderFactory, EndpointAddress address,
                FileTransportChannelFactory parent, Uri via)
        {
            this.via = via;
            this.path = via;
            this.writeFileName = FileTransportChannelUtils.RequestFileName;
            this.readFileName = FileTransportChannelUtils.ReplyFileName;
            this.bufferManager = bufferManager;
            this.messageEncoder = messageEncoderFactory.CreateSessionEncoder();
            this.remoteAddress = address;
            this.streamed = parent.Streamed;
            this.maxReceivedMessageSize = parent.MaxReceivedMessageSize;
            this.state = CommunicationState.Created;
        }

        public FileTransportChannel(BufferManager bufferManager,
            MessageEncoderFactory messageEncoderFactory, EndpointAddress address,
                FileTransportChannelListener parent)
        {
            this.localAddress = address;
            this.path = address.Uri;
            this.writeFileName = FileTransportChannelUtils.ReplyFileName;
            this.readFileName = FileTransportChannelUtils.RequestFileName;
            this.bufferManager = bufferManager;
            this.messageEncoder = messageEncoderFactory.CreateSessionEncoder();
            this.remoteAddress = address;
            this.streamed = parent.Streamed;
            this.maxReceivedMessageSize = parent.MaxReceivedMessageSize;
            this.state = CommunicationState.Created;
        }

        # endregion

        #region IInputChannel Members

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool returnValue = false;
            message = null;

            if (FileTransportChannelUtils.WaitForFileMessage(timeout,
                this.path.AbsolutePath, FileTransportChannelUtils.ReplyFileName))
            {
                returnValue = true;
                message = GetMessage();
            }
            return returnValue;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.readFileName == FileTransportChannelUtils.ReplyFileName && ! this.serverFileRead)
            {
                // If the Client is reading the reply message from the service 
                return waitForFileMessageDelegate.BeginInvoke(timeout, 
                    this.path.AbsolutePath, this.readFileName, callback, state);
            }
            else if (this.readFileName == FileTransportChannelUtils.RequestFileName && !this.clientFileRead)
            {
                // If the Server is reading the request message from the client
                GetMessageDelegate getMessageDelegate = new GetMessageDelegate(this.GetMessage);
                return getMessageDelegate.BeginInvoke(callback, state);
            }
            else
            {
                return null;
            }
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (this.readFileName == FileTransportChannelUtils.ReplyFileName)
            {
                // Read the message
                message = GetMessage();
                this.serverFileRead = true;
            }
            else
            {
                // Get the message previously read
                message = clientMessage;
                this.clientFileRead = true;
            }
            
            return true;
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(TimeSpan.FromMinutes
                (FileTransportChannelUtils.DefaultReceiveTimeout), callback, state);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message;
            if (this.TryReceive(timeout, out message))
            {
                return message;
            }
            else
            {
                throw new CommunicationException();
            }
        }

        public Message Receive()
        {
            return Receive(TimeSpan.FromMinutes 
                (FileTransportChannelUtils.DefaultReceiveTimeout));
        }

        public Message EndReceive(IAsyncResult result)
        {
            return GetMessage();
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        private Message GetMessage()
        {
            Message message;
            
            message = ReadMessageFromFile(
                PathToFile(this.path, this.readFileName));
            if (this.readFileName == FileTransportChannelUtils.RequestFileName)
            {
                clientMessage = message;
            }

            return message;
        }
        
        #endregion

        #region IOutputChannel Members

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Send(Message message, TimeSpan timeout)
        {
            try
            {
                FileTransportChannelUtils.CreateDirectoryIfNotPresent(this.path.AbsolutePath);
                                
                // Create a new file by this name in the 
                //  file system with the message body as its content
                WriteMessage(PathToFile(this.path, this.writeFileName), message);
            }
            catch (IOException exception)
            {
                throw FileTransportChannelUtils.ConvertException(exception);
            }
        }

        public void Send(Message message)
        {
            Send(message, TimeSpan.FromMinutes
                (FileTransportChannelUtils.DefaultSendTimeout));
        }

        public void EndSend(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IChannel Members

        public T GetProperty<T>() where T : class
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICommunicationObject Members

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Close(TimeSpan timeout)
        {
            //throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public event EventHandler Closed;

        public event EventHandler Closing;

        public void EndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void EndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public event EventHandler Faulted;

        public void Open(TimeSpan timeout)
        {
            this.state = CommunicationState.Opened;
            if (this.Opened != null)
            {
                this.Opened(this, null);
            }
        }

        public void Open()
        {
            Open(TimeSpan.FromMinutes(FileTransportChannelUtils.DefaultOpenTimeout));
        }

        public event EventHandler Opened;

        public event EventHandler Opening;

        public CommunicationState State
        {
            get { return this.state; }
        }

        #endregion

        # region Methods

        private Message ReadMessageFromFile(string path)
        {
            if (this.streamed)
            {
                return StreamedReadMessage(path);
            }
            return BufferedReadMessage(path);
        }

        private void WriteMessage(string path, Message message)
        {
            if (this.streamed)
            {
                StreamedWriteMessage(path, message);
            }
            else
            {
                BufferedWriteMessage(path, message);
            }
        }

        protected static string PathToFile(Uri path, String name)
        {
            UriBuilder address = new UriBuilder(path);
            address.Scheme = FileTransportChannelUtils.Scheme;
            address.Path = System.IO.Path.Combine(path.AbsolutePath, name);
            return address.Uri.AbsolutePath;
        }

        Message BufferedReadMessage(string path)
        {
            byte[] data;
            long totalBytes;

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    totalBytes = stream.Length;
                    if (totalBytes > int.MaxValue)
                    {
                        throw new CommunicationException(String.Format
                            (FileTransportChannelUtils.ErrMsgMaxBufferSizeExceeded,
                                totalBytes));
                    }

                    if (totalBytes > this.maxReceivedMessageSize)
                    {
                        throw new CommunicationException(String.Format
                            (FileTransportChannelUtils.ErrMsgMaxMsgSizeExceeded,
                                totalBytes, this.maxReceivedMessageSize));
                    }

                    data = this.bufferManager.TakeBuffer((int)totalBytes);
                    int bytesRead = 0;
                    while (bytesRead < totalBytes)
                    {
                        int count = stream.Read(data, bytesRead, (int)totalBytes - bytesRead);
                        if (count == 0)
                        {
                            throw new CommunicationException(String.Format
                                (FileTransportChannelUtils.ErrMsgUnexpectedEndOfMsg,
                                    bytesRead, totalBytes));
                        }
                        bytesRead += count;
                    }
                }
            }
            catch (IOException exception)
            {
                throw FileTransportChannelUtils.ConvertException(exception);
            }
            ArraySegment<byte> buffer = new ArraySegment<byte>(data, 0, (int)totalBytes);
            return this.messageEncoder.ReadMessage(buffer, this.bufferManager);
        }

        void BufferedWriteMessage(string path, Message message)
        {
            ArraySegment<byte> buffer;
            using (message)
            {
                this.remoteAddress.ApplyTo(message);
                buffer = this.messageEncoder.WriteMessage(message,
                    FileTransportChannelUtils.MaxSizeOfHeaders, this.bufferManager);
            }

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                }
            }
            catch (IOException exception)
            {
                throw FileTransportChannelUtils.ConvertException(exception);
            }
        }

        Message StreamedReadMessage(string path)
        {
            try
            {
                FileStream stream = File.Open(path, FileMode.Open);
                long totalBytes = stream.Length;
                if (totalBytes > this.maxReceivedMessageSize)
                {
                    throw new CommunicationException(String.Format
                        (FileTransportChannelUtils.ErrMsgMaxMsgSizeExceeded,
                            totalBytes, this.maxReceivedMessageSize));
                }
                return this.messageEncoder.ReadMessage(stream,
                    FileTransportChannelUtils.MaxSizeOfHeaders);
            }
            catch (IOException exception)
            {
                throw FileTransportChannelUtils.ConvertException(exception);
            }
        }

        void StreamedWriteMessage(string path, Message message)
        {
            using (message)
            {
                this.remoteAddress.ApplyTo(message);
                try
                {
                    using (Stream stream = File.Open(path, FileMode.Create))
                    {
                        this.messageEncoder.WriteMessage(message, stream);
                    }
                }
                catch (IOException exception)
                {
                    throw FileTransportChannelUtils.ConvertException(exception);
                }
            }
        }

        # endregion
    }
}
