using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace NServiceBus.FileBasedRouting
{
    public class XmlRoutingFileParser
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

                config.Commands = GetAllMessages(handles, "command", "commands").ToArray();
                config.Events = GetAllMessages(handles, "event", "events").ToArray();
                configs.Add(config);
            }

            return configs;
        }

        static IEnumerable<Type> GetAllMessages(XElement handles, string singularElementName, string pluralElementName)
        {
            var separatelyConfiguredCommands = handles
                ?.Elements(singularElementName)
                .Select(e => SelectCommand(e.Attribute("type").Value))
                .ToArray() ?? Type.EmptyTypes;

            var filteredCommands = handles?.Elements(pluralElementName).SelectMany(SelectMessages) ?? Type.EmptyTypes;
            var allCommands = separatelyConfiguredCommands.Concat(filteredCommands).Distinct();
            return allCommands;
        }

        static Type SelectCommand(string typeName)
        {
            return Type.GetType(typeName, true);
        }

        static IEnumerable<Type> SelectMessages(XElement commandsElement)
        {
            var assemblyName = commandsElement.Attribute("assembly").Value;
            var assembly = Assembly.Load(assemblyName);
            var exportedTypes = assembly.ExportedTypes;
            var @namespace = commandsElement.Attribute("namespace");
            if (@namespace == null)
            {
                return exportedTypes;
            }
            return
                exportedTypes.Where(
                    type => type.Namespace != null && type.Namespace.StartsWith(@namespace.Value));
        }


        readonly XDocument document;
        readonly XmlSchemaSet schema;
    }
}