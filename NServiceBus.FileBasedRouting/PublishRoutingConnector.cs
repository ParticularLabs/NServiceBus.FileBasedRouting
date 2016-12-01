using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Routing;

namespace NServiceBus.FileBasedRouting
{
    class PublishRoutingConnector : StageConnector<IOutgoingPublishContext, IOutgoingLogicalMessageContext>
    {
        readonly RoutingTable routingTable;
        readonly EndpointInstances endpointInstances;
        private readonly IDistributionPolicy distributionPolicy;
        private readonly Func<EndpointInstance, string> resolveTransportAddress;

        public PublishRoutingConnector(RoutingTable routingTable, EndpointInstances endpointInstances, IDistributionPolicy distributionPolicy, Func<EndpointInstance, string> resolveTransportAddress)
        {
            this.routingTable = routingTable;
            this.endpointInstances = endpointInstances;
            this.distributionPolicy = distributionPolicy;
            this.resolveTransportAddress = resolveTransportAddress;
        }

        public override Task Invoke(IOutgoingPublishContext context, Func<IOutgoingLogicalMessageContext, Task> stage)
        {
            var logicalEndpoints = routingTable.Endpoints
                .Where(endpoint => endpoint.Events.Any(@event => @event.IsAssignableFrom(context.Message.MessageType)))
                .Select(endpoint => endpoint.LogicalEndpointName);

            HashSet<string> receivers = new HashSet<string>();
            foreach (var logicalEndpoint in logicalEndpoints)
            {
                var instances = endpointInstances.FindInstances(logicalEndpoint);
                var policy = distributionPolicy.GetDistributionStrategy(
                    logicalEndpoint,
                    DistributionStrategyScope.Publish);

                receivers.Add(policy
                    .SelectReceiver(instances.Select(resolveTransportAddress)
                    .ToArray()));
            }

            var routingStrategies = receivers
                .Select(r => new UnicastRoutingStrategy(r))
                .ToArray();

            var outgoingMessageContext = this.CreateOutgoingLogicalMessageContext(
                context.Message,
                routingStrategies,
                context);
            return stage(outgoingMessageContext);
        }
    }
}