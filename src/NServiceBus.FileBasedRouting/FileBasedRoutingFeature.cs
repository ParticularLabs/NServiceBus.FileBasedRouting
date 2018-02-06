using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Routing;
using NServiceBus.Transport;
using NServiceBus.Logging;

namespace NServiceBus.FileBasedRouting
{
    class FileBasedRoutingFeature : Feature
    {
        static ILog log = LogManager.GetLogger<FileBasedRoutingFeature>();

        public const string RoutingFilePathKey = "NServiceBus.FileBasedRouting.RoutingFileUri";
        public const string RouteFileUpdateInterval = "NServiceBus.FileBasedRouting.RouteFileUpdateInterval";
        internal const string MessageDrivenPubSubEnabled = "NServiceBus.FileBasedRouting.MessageDrivenPubSubEnabled";

        public FileBasedRoutingFeature()
        {
            Defaults(s =>
            {
                s.SetDefault(RoutingFilePathKey, UriHelper.FilePathToUri("endpoints.xml"));
                s.SetDefault(RouteFileUpdateInterval, TimeSpan.FromSeconds(30));
                s.SetDefault(MessageDrivenPubSubEnabled, false);
                s.SetDefault<UnicastSubscriberTable>(new UnicastSubscriberTable());
            });
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var transportInfrastructure = context.Settings.Get<TransportInfrastructure>();

            var unicastRoutingTable = context.Settings.Get<UnicastRoutingTable>();
            var unicastSubscriberTable = context.Settings.Get<UnicastSubscriberTable>();

            var routeFileUpdateInterval = context.Settings.Get<TimeSpan>(RouteFileUpdateInterval);
            var routingFileUri = context.Settings.Get<Uri>(RoutingFilePathKey);
            var routingFile = new XmlRoutingFileAccess(routingFileUri);
            var routingFileParser = new XmlRoutingFileParser();

            var nativeSends = transportInfrastructure.OutboundRoutingPolicy.Sends == OutboundRoutingType.Multicast;
            var nativePublishes = transportInfrastructure.OutboundRoutingPolicy.Publishes == OutboundRoutingType.Multicast;

            // ensure the routing file is valid and the routing table is populated before running FeatureStartupTasks
            UpdateRoutingTable(routingFileParser, routingFile, unicastRoutingTable, unicastSubscriberTable, nativeSends, nativePublishes);

            if (routeFileUpdateInterval > TimeSpan.Zero)
            {
                context.RegisterStartupTask(new UpdateRoutingTask(() => UpdateRoutingTable(routingFileParser, routingFile, unicastRoutingTable, unicastSubscriberTable, nativeSends, nativePublishes), routeFileUpdateInterval));
            }

            // if the transport provides native pub/sub support, don't plug in the FileBased pub/sub storage.
            if (context.Settings.Get<TransportInfrastructure>().OutboundRoutingPolicy.Publishes == OutboundRoutingType.Unicast)
            {
                var routingConnector = new PublishRoutingConnector(
                    unicastSubscriberTable,
                    context.Settings.Get<EndpointInstances>(),
                    context.Settings.Get<DistributionPolicy>(),
                    instance => transportInfrastructure.ToTransportAddress(LogicalAddress.CreateRemoteAddress(instance)));

                context.Pipeline.Replace("UnicastPublishRouterConnector", routingConnector);
                if (!context.Settings.Get<bool>(MessageDrivenPubSubEnabled))
                {
                    context.Pipeline.Replace("MessageDrivenSubscribeTerminator", new SubscribeTerminator(), "handles subscribe operations");
                    context.Pipeline.Replace("MessageDrivenUnsubscribeTerminator", new UnsubscribeTerminator(), "handles ubsubscribe operations");
                }

            }
        }

        static void UpdateRoutingTable(XmlRoutingFileParser routingFileParser, XmlRoutingFileAccess routingFile, UnicastRoutingTable routingTable, UnicastSubscriberTable subscriberTable, bool nativeSends, bool nativePublishes)
        {
            try
            {
                var endpoints = routingFileParser.Parse(routingFile.Read());

                var commandRoutes = new List<RouteTableEntry>();
                var eventRoutes = new List<RouteTableEntry>();

                foreach (var endpoint in endpoints)
                {
                    var route = UnicastRoute.CreateFromEndpointName(endpoint.LogicalEndpointName);
                    foreach (var commandType in endpoint.Commands)
                    {
                        if (nativeSends)
                        {
                            log.Warn($"Selected transport uses native command routing. Route for {commandType.FullName} to {endpoint.LogicalEndpointName} configured in {routingFile.FileUri} will be ignored.");
                        }
                        commandRoutes.Add(new RouteTableEntry(commandType, route));
                    }

                    foreach (var eventType in endpoint.Events)
                    {
                        if (nativePublishes)
                        {
                            log.Warn($"Selected transport uses native event routing. Route for {eventType.FullName} to {endpoint.LogicalEndpointName} configured in {routingFile.FileUri} will be ignored.");
                        }
                        eventRoutes.Add(new RouteTableEntry(eventType, route));
                    }
                }

                routingTable.AddOrReplaceRoutes("FileBasedRouting", commandRoutes);
                subscriberTable.AddOrReplaceRoutes("FileBasedRouting", eventRoutes);

                log.Debug($"Updated routing information from {routingFile.FileUri}");
            }
            catch (Exception e)
            {
                log.Error($"Failed to update routing information from {routingFile.FileUri}. The last valid routing configuration will be used instead.", e);
                throw;
            }
        }

        class UpdateRoutingTask : FeatureStartupTask, IDisposable
        {
            Action updateRoutingCallback;
	        TimeSpan routeFileUpdateInterval;
	        Timer updateTimer;

            public UpdateRoutingTask(Action updateRoutingCallback, TimeSpan routeFileUpdateInterval)
            {
	            this.updateRoutingCallback = updateRoutingCallback;
	            this.routeFileUpdateInterval = routeFileUpdateInterval;
            }

            protected override Task OnStart(IMessageSession session)
            {
                updateTimer = new Timer(state =>
                {
                    try
                    {
                        updateRoutingCallback();
                    }
                    catch (Exception)
                    {
                        // ignore exceptions to prevent endpoint crashes
                    }
                }, null, routeFileUpdateInterval, routeFileUpdateInterval);

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