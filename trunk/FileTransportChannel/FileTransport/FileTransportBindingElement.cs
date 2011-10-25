
namespace FileTransport
{
    # region using

    using System;
    using System.ServiceModel.Channels;
    using System.Globalization;

    # endregion

    public class FileTransportBindingElement : TransportBindingElement
    {
        # region member_variables
        
        private bool streamed;

        # endregion

        # region Properties

        public bool Streamed
        {
            get { return streamed; }
            set { streamed = value; }
        }

        public override string Scheme
        {
            get { return FileTransportChannelUtils.Scheme; }
        }

        # endregion

        # region Constructor

        public FileTransportBindingElement()
        {
            this.Streamed = false;
        }

        public FileTransportBindingElement(FileTransportBindingElement other) 
        {
            this.Streamed = other.Streamed;
        }

        # endregion

        # region TransportBindingElement overridden methods

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return (typeof(TChannel) == typeof(IDuplexChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return (typeof(TChannel) == typeof(IDuplexChannel));
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>
            (BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            else if (!CanBuildChannelFactory<TChannel>(context))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    FileTransportChannelUtils.ErrMsgUnsupportedMessageType, 
                        typeof(TChannel).Name));
            }
            else
            {
                return (IChannelFactory<TChannel>)
                    (object)new FileTransportChannelFactory(this, context);
            }
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>
            (BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            else if (!CanBuildChannelListener<TChannel>(context))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    FileTransportChannelUtils.ErrMsgUnsupportedMessageType, 
                        typeof(TChannel).Name));
            }
            else
            {
                return (IChannelListener<TChannel>)
                    (object)new FileTransportChannelListener(this, context);
            }
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return context.GetInnerProperty<T>();
        }

        public override BindingElement Clone()
        {
            return new FileTransportBindingElement(this);
        }

        # endregion
    }
}
