using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NServiceBus.Features;
using NServiceBus.Routing;

namespace NServiceBus.FileBasedRouting
{
    public class FileBasedRoutingFeature : Feature
    {
        const string RoutingFilePathKey = "NServiceBus.FileBasedRouting.RoutingFilePath";

        public FileBasedRoutingFeature()
        {
            Defaults(s =>
                s.SetDefault(RoutingFilePathKey, "endpoints.xml"));
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var unicastRoutingTable = context.Settings.Get<UnicastRoutingTable>();
            XDocument document;
            using (var fileStream = File.OpenRead(context.Settings.Get<string>(RoutingFilePathKey)))
            {
                document = XDocument.Load(fileStream);
            }

            var routingFile = new XmlRoutingFileParser(document);

            // ensure the routing file is valid and the routing table is populated before running FeatureStartupTasks
            UpdateRoutingTable(routingFile, unicastRoutingTable);

            context.RegisterStartupTask(new UpdateRoutingTask(routingFile, unicastRoutingTable));
        }

        private static void UpdateRoutingTable(XmlRoutingFileParser routingFileParser, UnicastRoutingTable unicastRoutingTable)
        {
            var endpoints = routingFileParser.Read();

            var commandRoutes = new List<RouteTableEntry>();
            foreach (var endpoint in endpoints)
            {
                foreach (var command in endpoint.Commands)
                {
                    commandRoutes.Add(new RouteTableEntry(command,
                        UnicastRoute.CreateFromEndpointName(endpoint.LogicalEndpointName)));
                }
            }

            unicastRoutingTable.AddOrReplaceRoutes("FileBasedRouting", commandRoutes);
        }

        class UpdateRoutingTask : FeatureStartupTask, IDisposable
        {
            private readonly XmlRoutingFileParser routingFileParser;
            private readonly UnicastRoutingTable unicastRoutingTable;
            private Timer updateTimer;

            public UpdateRoutingTask(XmlRoutingFileParser routingFileParser, UnicastRoutingTable unicastRoutingTable)
            {
                this.routingFileParser = routingFileParser;
                this.unicastRoutingTable = unicastRoutingTable;
            }

            protected override Task OnStart(IMessageSession session)
            {
                updateTimer = new Timer(state => UpdateRoutingTable(routingFileParser, unicastRoutingTable), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

                return Task.CompletedTask;
            }

            protected override Task OnStop(IMessageSession session)
            {
                updateTimer?.Dispose();
                updateTimer = null;

                return Task.CompletedTask;
            }

            public void Dispose()
            {
                updateTimer?.Dispose();
                updateTimer = null;
            }
        }
    }
}