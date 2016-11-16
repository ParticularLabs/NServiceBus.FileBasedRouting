using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NServiceBus.FileBasedRouting
{
    class XmlRoutingFile
    {
        private readonly string filePath;

        public XmlRoutingFile(string filePath)
        {
            this.filePath = filePath;
        }

        public IEnumerable<EndpointRoutingConfiguration> Read()
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var document = XDocument.Load(fileStream);
                var endpointElements = document.Root.Descendants("endpoint");

                var configs = new List<EndpointRoutingConfiguration>();
                foreach (var endpointElement in endpointElements)
                {
                    var config = new EndpointRoutingConfiguration();
                    config.LogicalEndpointName = endpointElement.Attribute("name").Value;

                    config.Commands = endpointElement.Element("handles")
                        ?.Elements("command")
                        .Select(e => Type.GetType(e.Attribute("type").Value, true))
                        .ToArray() ?? new Type[0];

                    configs.Add(config);
                }

                return configs;
            }
        }
    }
}