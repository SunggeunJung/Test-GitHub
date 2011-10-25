
namespace FileTransport
{
    # region using

    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    # endregion

    class FileTransportElement : BindingElementExtensionElement
    {
        # region Constructor

        public FileTransportElement()
        {
        }

        # endregion

        # region Properties

        public override Type BindingElementType
        {
            get { return typeof(FileTransportBindingElement); }
        }
        
        # region Configuration Properties

        [ConfigurationProperty(FileTransportChannelUtils.MaxBufferPoolSizeString, 
            DefaultValue = FileTransportChannelUtils.MaxBufferPoolSize)]
        [LongValidator(MinValue=0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[FileTransportChannelUtils.MaxBufferPoolSizeString]; }
            set { base[FileTransportChannelUtils.MaxBufferPoolSizeString] = value; }
        }

        [ConfigurationProperty(FileTransportChannelUtils.MaxReceivedMessageSizeString,
            DefaultValue = FileTransportChannelUtils.MaxReceivedMessageSize)]
        [LongValidator(MinValue=0)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[FileTransportChannelUtils.MaxReceivedMessageSizeString]; }
            set { base[FileTransportChannelUtils.MaxReceivedMessageSizeString] = value; }
        }

        [ConfigurationProperty(FileTransportChannelUtils.StreamedString,
            DefaultValue = FileTransportChannelUtils.Streamed)]
        public bool Streamed
        {
            get { return (bool)base[FileTransportChannelUtils.StreamedString]; }
            set { base[FileTransportChannelUtils.StreamedString] = value; }
        }

        # endregion

        # endregion

        # region Configuration method overrides

        protected override BindingElement CreateBindingElement()
        {
            FileTransportBindingElement fileTransportElement = new FileTransportBindingElement();
            this.ApplyConfiguration(fileTransportElement);
            return fileTransportElement;
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            FileTransportBindingElement fileTransportElement = (FileTransportBindingElement)bindingElement;
            fileTransportElement.MaxBufferPoolSize = this.MaxBufferPoolSize;
            fileTransportElement.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            fileTransportElement.Streamed = this.Streamed;
        }

        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            FileTransportBindingElement fileTransportElement = (FileTransportBindingElement)bindingElement;
            this.MaxBufferPoolSize = fileTransportElement.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = fileTransportElement.MaxReceivedMessageSize;
            this.Streamed = fileTransportElement.Streamed;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            FileTransportElement source = (FileTransportElement)from;
            this.MaxBufferPoolSize = source.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = source.MaxReceivedMessageSize;
            this.Streamed = source.Streamed;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection properties = base.Properties;
                properties.Add(new ConfigurationProperty(FileTransportChannelUtils.MaxBufferPoolSizeString,
                    typeof(long), FileTransportChannelUtils.MaxBufferPoolSize,
                    null, new LongValidator(0, Int64.MaxValue), 
                    ConfigurationPropertyOptions.None));
                properties.Add(new ConfigurationProperty(FileTransportChannelUtils.MaxReceivedMessageSizeString,
                    typeof(long), FileTransportChannelUtils.MaxReceivedMessageSize,
                    null, new LongValidator(0, Int64.MaxValue),
                    ConfigurationPropertyOptions.None));
                properties.Add(new ConfigurationProperty(FileTransportChannelUtils.StreamedString,
                    typeof(bool), FileTransportChannelUtils.Streamed,
                    null, null,ConfigurationPropertyOptions.None));
                return properties;
            }
        }

        # endregion
    }
}
