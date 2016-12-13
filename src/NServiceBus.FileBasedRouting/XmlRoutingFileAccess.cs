namespace NServiceBus.FileBasedRouting
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    class XmlRoutingFileAccess
    {
        public string FilePath { get; }

        public XmlRoutingFileAccess(string filePath)
        {
            FilePath = filePath;
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