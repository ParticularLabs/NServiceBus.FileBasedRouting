using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace NServiceBus.FileBasedRouting
{
    class XmlRoutingFileParser
    {
        public XmlRoutingFileParser(XDocument document)
        {
            this.document = document;

            using (var stream = GetType().Assembly.GetManifestResourceStream("NServiceBus.FileBasedRouting.routing.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                schema = new XmlSchemaSet();
                schema.Add("", xmlReader);
            }
        }

        public IEnumerable<EndpointRoutingConfiguration> Read()
        {
            document.Validate(schema, null, true);

            var endpointElements = document.Root.Descendants("endpoint");

            var configs = new List<EndpointRoutingConfiguration>();
            foreach (var endpointElement in endpointElements)
            {
                var config = new EndpointRoutingConfiguration
                {
                    LogicalEndpointName = endpointElement.Attribute("name").Value
                };

                var handles = endpointElement.Element("handles");

                var separatelyConfiguredCommands = handles
                                                       ?.Elements("command")
                                                       .Select(e => SelectCommand(e.Attribute("type").Value))
                                                       .ToArray() ?? Type.EmptyTypes;

                var filteredCommands = handles?.Elements("commands").SelectMany(SelectCommands) ?? Type.EmptyTypes;

                config.Commands = separatelyConfiguredCommands.Concat(filteredCommands).Distinct().ToArray();
                configs.Add(config);
            }

            return configs;
        }

        static Type SelectCommand(string typeName)
        {
            return Type.GetType(typeName, true);
        }

        static IEnumerable<Type> SelectCommands(XElement commandsElement)
        {
            var assemblyName = commandsElement.Attribute("assembly").Value;
            var assembly = Assembly.Load(assemblyName);
            var exportedTypes = assembly.ExportedTypes;
            var @namespace = commandsElement.Attribute("namespace");
            if (@namespace == null)
            {
                return exportedTypes;
            }

            // the namespace attribute exists, but it's empty
            if (string.IsNullOrEmpty(@namespace.Value))
            {
                // return only types with no namespace at all
                return exportedTypes.Where(type => string.IsNullOrEmpty(type.Namespace));
            }

            return exportedTypes.Where(type => type.Namespace != null && type.Namespace.StartsWith(@namespace.Value));
        }

        readonly XDocument document;
        readonly XmlSchemaSet schema;
    }
}