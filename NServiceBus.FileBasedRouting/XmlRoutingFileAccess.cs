namespace NServiceBus.FileBasedRouting
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public class XmlRoutingFileAccess
    {
        readonly string filePath;

        public XmlRoutingFileAccess(string filePath)
        {
            this.filePath = filePath;
        }

        public XDocument Read()
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    return XDocument.Load(fileStream);
                }
            }
            catch (XmlException e)
            {
                throw new Exception("The configured routing file is no valid XML file.", e);
            }
        }
    }
}