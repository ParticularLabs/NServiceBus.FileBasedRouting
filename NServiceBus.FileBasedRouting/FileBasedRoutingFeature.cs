using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Routing;
using NServiceBus.Transport;

namespace NServiceBus.FileBasedRouting
{
    public class FileBasedRoutingFeature : Feature
    {
        const string RoutingFilePathKey = "NServiceBus.FileBasedRouting.RoutingFilePath";

        public FileBasedRoutingFeature()
        {
            Defaults(s =>
            {
                s.SetDefault(RoutingFilePathKey, "endpoints.xml");
                s.SetDefault<UnicastSubscriberTable>(new UnicastSubscriberTable());
            });
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var unicastRoutingTable = context.Settings.Get<UnicastRoutingTable>();
            var unicastSubscriberTable = context.Settings.Get<UnicastSubscriberTable>();

            var routingFile = new XmlRoutingFileAccess(context.Settings.Get<string>(RoutingFilePathKey));
            var routingFileParser = new XmlRoutingFileParser();

            // ensure the routing file is valid and the routing table is populated before running FeatureStartupTasks
            UpdateRoutingTable(routingFileParser, routingFile, unicastRoutingTable, unicastSubscriberTable);

            context.RegisterStartupTask(new UpdateRoutingTask(routingFileParser, routingFile, unicastRoutingTable, unicastSubscriberTable));

            // if the transport provides native pub/sub support, don't plug in the FileBased pub/sub storage.
            if (context.Settings.Get<TransportInfrastructure>().OutboundRoutingPolicy.Publishes == OutboundRoutingType.Unicast)
            {
                var transportInfrastructure = context.Settings.Get<TransportInfrastructure>();
                var routingConnector = new PublishRoutingConnector(
                    unicastSubscriberTable,
                    context.Settings.Get<EndpointInstances>(),
                    context.Settings.Get<DistributionPolicy>(),
                    instance => transportInfrastructure.ToTransportAddress(LogicalAddress.CreateRemoteAddress(instance)));

                context.Pipeline.Replace("UnicastPublishRouterConnector", routingConnector);
                context.Pipeline.Replace("MessageDrivenSubscribeTerminator", new SubscribeTerminator(), "handles subscribe operations");
                context.Pipeline.Replace("MessageDrivenUnsubscribeTerminator", new UnsubscribeTerminator(), "handles ubsubscribe operations");

            }
        }

        static void UpdateRoutingTable(XmlRoutingFileParser routingFileParser, XmlRoutingFileAccess routingFile, UnicastRoutingTable routingTable, UnicastSubscriberTable subscriberTable)
        {
            var endpoints = routingFileParser.Parse(routingFile.Read());

            var commandRoutes = new List<RouteTableEntry>();
            var eventRoutes = new List<RouteTableEntry>();

            foreach (var endpoint in endpoints)
            {
                foreach (var commandType in endpoint.Commands)
                {
                    commandRoutes.Add(new RouteTableEntry(commandType, UnicastRoute.CreateFromEndpointName(endpoint.LogicalEndpointName)));
                }

                foreach (var eventType in endpoint.Events)
                {
                    eventRoutes.Add(new RouteTableEntry(eventType, UnicastRoute.CreateFromEndpointName(endpoint.LogicalEndpointName)));
                }
            }

            routingTable.AddOrReplaceRoutes("FileBasedRouting", commandRoutes);
            subscriberTable.AddOrReplaceRoutes("FileBasedRouting", eventRoutes);
        }

        class UpdateRoutingTask : FeatureStartupTask, IDisposable
        {
            XmlRoutingFileParser routingFileParser;
            XmlRoutingFileAccess routingFile;
            UnicastRoutingTable unicastRoutingTable;
            UnicastSubscriberTable subscriberTable;
            Timer updateTimer;

            public UpdateRoutingTask(XmlRoutingFileParser routingFileParser, XmlRoutingFileAccess routingFile, UnicastRoutingTable unicastRoutingTable, UnicastSubscriberTable subscriberTable)
            {
                this.routingFileParser = routingFileParser;
                this.routingFile = routingFile;
                this.unicastRoutingTable = unicastRoutingTable;
                this.subscriberTable = subscriberTable;
            }

            protected override Task OnStart(IMessageSession session)
            {
                updateTimer = new Timer(state => UpdateRoutingTable(routingFileParser, routingFile, unicastRoutingTable, subscriberTable), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

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