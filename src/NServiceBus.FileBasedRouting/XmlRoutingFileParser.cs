using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NServiceBus.Logging;

namespace NServiceBus.FileBasedRouting
{
    class XmlRoutingFileParser
    {
        public XmlRoutingFileParser()
        {
            logger = LogManager.GetLogger(typeof(XmlRoutingFileParser));

            using (var stream = GetType().Assembly.GetManifestResourceStream("NServiceBus.FileBasedRouting.routing.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                schema = new XmlSchemaSet();
                schema.Add("", xmlReader);
            }
        }

        public IEnumerable<EndpointRoutingConfiguration> Parse(XDocument document)
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

                config.Commands = GetAllMessageTypes(endpointElement, "command", "commands").ToArray();
                config.Events = GetAllMessageTypes(endpointElement, "event", "events").ToArray();
                configs.Add(config);
            }

            return configs;
        }

        IEnumerable<Type> GetAllMessageTypes(XElement endpointElement, string singular, string plural)
        {
            var handles = endpointElement.Element("handles");

            var separatelyConfiguredCommands = handles
                ?.Elements(singular)
                .Select(e => FindMessageType(e.Attribute("type").Value))
                .Where(e => e != null)
                .ToArray() ?? Type.EmptyTypes;

            var filteredCommands = handles?.Elements(plural).SelectMany(SelectMessages) ?? Type.EmptyTypes;
            var allCommands = separatelyConfiguredCommands.Concat(filteredCommands).Distinct();
            return allCommands;
        }

        Type FindMessageType(string typeName)
        {
            Type msg = null;

            try
            {
                msg = Type.GetType(typeName, true);
            }
            catch
            {
                //ignore
            }

            if (msg == null)
            {
                logger.Warn($"Cannot add route for unknown type {typeName}.");
            }

            return msg;
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

            // the namespace attribute exists, but it's empty
            if (string.IsNullOrEmpty(@namespace.Value))
            {
                // return only types with no namespace at all
                return exportedTypes.Where(type => string.IsNullOrEmpty(type.Namespace));
            }

            return exportedTypes.Where(type => type.Namespace != null && type.Namespace.StartsWith(@namespace.Value));
        }

        readonly XmlSchemaSet schema;
        readonly ILog logger;
    }
}