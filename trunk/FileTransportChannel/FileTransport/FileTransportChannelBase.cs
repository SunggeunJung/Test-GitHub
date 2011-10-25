
namespace FileTransport
{
    # region using

    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    # endregion

    abstract class FileTransportChannelBase1 : ChannelBase
    {
        # region member_variables

        private BufferManager bufferManager;
        private MessageEncoder messageEncoder;
        private EndpointAddress remoteAddress;
        private bool streamed;
        readonly long maxReceivedMessageSize;

        # endregion

        # region Properties

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

        # region Constructor

        public FileTransportChannelBase(BufferManager bufferManager,
                MessageEncoderFactory messageEncoderFactory, EndpointAddress address,
                ChannelManagerBase parent, bool streamed, long maxReceivedMessageSize)
            : base(parent)
        {
            this.bufferManager = bufferManager;
            this.messageEncoder = messageEncoderFactory.CreateSessionEncoder();
            this.remoteAddress = address;
            this.streamed = streamed;
            this.maxReceivedMessageSize = maxReceivedMessageSize;
        }

        internal FileTransportChannelBase(ChannelManagerBase listner) : base(listner)
        { 
        }

        # endregion

        # region Methods

        protected Message ReadMessageFromFile(string path)
        {
            if (this.streamed)
            {
                return StreamedReadMessage(path);
            }
            return BufferedReadMessage(path);
        }

        protected void WriteMessage(string path, Message message)
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
            address.Path = Path.Combine(path.AbsolutePath, name);
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
                Stream stream = File.Open(path, FileMode.Open);
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