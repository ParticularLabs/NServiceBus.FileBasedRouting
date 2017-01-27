namespace NServiceBus.FileBasedRouting
{
    using System;
    using System.Xml;
    using System.Xml.Linq;

    class XmlRoutingFileAccess
    {
        public Uri FileUri { get; }

        public XmlRoutingFileAccess(Uri fileUri)
        {
            FileUri = fileUri;
        }

        public XDocument Read()
        {
            try
            {
                return XDocument.Load(FileUri.ToString());
            }
            catch (XmlException e)
            {
                throw new Exception("The configured routing file is no valid XML file.", e);
            }
        }
    }
}