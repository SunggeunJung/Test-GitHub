
namespace FileTransport
{
    # region using

    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.ServiceModel.Samples;

    # endregion

    static class FileTransportChannelUtils
    {
        # region Constants

        internal const int MaxSizeOfHeaders = 4 * 1024;
        internal const long MaxBufferPoolSize = 64 * 1024;
        internal const long MaxReceivedMessageSize = 5 * 1024 * 1024;
        internal const bool Streamed = false;

        internal const int DefaultReceiveTimeout = 5;
        internal const int DefaultSendTimeout = 5;
        internal const int DefaultOpenTimeout = 5;

        internal const string Scheme = "my.file";
        internal const string RequestFileName = "RequestFile";
        internal const string ReplyFileName = "ReplyFile";
        internal const string ReverseStringFolderName = "ReverseStringFolder";

        public const string MaxSizeOfHeadersString = "maxSizeOfHeaders";
        public const string MaxBufferPoolSizeString = "maxBufferPoolSize";
        public const string MaxReceivedMessageSizeString = "MaxReceivedMessageSize";
        public const string StreamedString = "streamed";

        # endregion

        # region Messages

        internal const string ErrMsgUnsupportedMessageType =
            "Unsupported channel type: {0}" ;
        internal const string ErrMsgMaxMsgSizeExceeded =
            "Message exceeded maximum size : {0} > {1}.";
        internal const string ErrMsgUnexpectedEndOfMsg = 
            "Unexpected end of message after {0} of {1} bytes.";
        internal const string ErrMsgMaxBufferSizeExceeded =
            "Message of size {0} is too large to buffer. Use a streamed transfer instead";

        # endregion

        # region Properties

        static MessageEncoderFactory messageEncoderFactory;

        internal static MessageEncoderFactory DefaultMessageEncoderFactory
        {
            get
            {
                return messageEncoderFactory;
            }
        }

        # endregion

        # region Constructor

        static FileTransportChannelUtils()
        {
            messageEncoderFactory = 
                new GZipMessageEncodingBindingElement().CreateMessageEncoderFactory();
        }

        # endregion

        # region Methods

        internal static void CreateDirectoryIfNotPresent(string absolutePath)
        {
            // Create a Directory if it does not exist already
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        internal static bool WaitForFileMessage
            (TimeSpan timeout, string absolutePath, string fileName)
        {
            try
            {
                using (FileSystemWatcher watcher =
                        new FileSystemWatcher(absolutePath, fileName))
                {
                    watcher.EnableRaisingEvents = true;
                    WaitForChangedResult result;

                    int time;
                    time = (timeout.TotalMilliseconds > Int32.MaxValue)
                        ? Int32.MaxValue : timeout.Milliseconds;

                    result = watcher.WaitForChanged(WatcherChangeTypes.Changed, time);

                    return !result.TimedOut;
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        internal static bool WaitForDirectoryChannel
            (TimeSpan timeout, string directoryName)
        {
            try
            {
                using (FileSystemWatcher watcher =
                        new FileSystemWatcher("C:\\", directoryName))
                {
                    watcher.EnableRaisingEvents = true;
                    WaitForChangedResult result;
                    
                    int time;
                    time = (timeout.TotalMilliseconds > Int32.MaxValue) 
                        ? Int32.MaxValue : timeout.Milliseconds;

                    result = watcher.WaitForChanged(WatcherChangeTypes.Created, time);

                    return !result.TimedOut;
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        internal static Exception ConvertException(Exception exception)
        {
            Type exceptionType = exception.GetType();
            if (exceptionType == typeof(DirectoryNotFoundException) ||
                    exceptionType == typeof(FileNotFoundException) ||
                        exceptionType == typeof(PathTooLongException))
            {
                return new EndpointNotFoundException(exception.Message, exception);
            }
            return new CommunicationException(exception.Message, exception);
        }

        # endregion
    }
}
