namespace NServiceBus.FileBasedRouting
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public class XmlRoutingFileAccess
    {
        public string FilePath { get; }

        public XmlRoutingFileAccess(string filePath)
        {
            this.FilePath = filePath;
        }

        public XDocument Read()
        {
            try
            {
                using (var fileStream = File.OpenRead(FilePath))
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