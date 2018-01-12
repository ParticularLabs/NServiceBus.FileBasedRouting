using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Routing;
using NServiceBus.Unicast.Queuing;

namespace NServiceBus.FileBasedRouting
{
    class PublishRoutingConnector : StageConnector<IOutgoingPublishContext, IOutgoingLogicalMessageContext>
    {
        readonly UnicastSubscriberTable routingTable;
        readonly EndpointInstances endpointInstances;
        readonly IDistributionPolicy distributionPolicy;
        readonly Func<EndpointInstance, string> resolveTransportAddress;

        public PublishRoutingConnector(UnicastSubscriberTable routingTable, EndpointInstances endpointInstances, IDistributionPolicy distributionPolicy, Func<EndpointInstance, string> resolveTransportAddress)
        {
            this.routingTable = routingTable;
            this.endpointInstances = endpointInstances;
            this.distributionPolicy = distributionPolicy;
            this.resolveTransportAddress = resolveTransportAddress;
        }

        public override async Task Invoke(IOutgoingPublishContext context, Func<IOutgoingLogicalMessageContext, Task> stage)
        {
            var eventType = context.Message.MessageType;
            var routingStrategies = Route(eventType, context).ToList();
            if (routingStrategies.Count == 0)
            {
                //No subscribers for this message.
                return;
            }

            context.Headers[Headers.MessageIntent] = MessageIntentEnum.Publish.ToString();

            try
            {
                await stage(this.CreateOutgoingLogicalMessageContext(context.Message, routingStrategies, context)).ConfigureAwait(false);
            }
            catch (QueueNotFoundException ex)
            {
                throw new Exception($"The destination queue '{ex.Queue}' could not be found. The destination may be misconfigured for this kind of message ({eventType}) in the MessageEndpointMappings of the UnicastBusConfig section in the configuration file. It may also be the case that the given queue hasn\'t been created yet, or has been deleted.", ex);
            }
        }

        IEnumerable<UnicastRoutingStrategy> Route(Type messageType, IOutgoingPublishContext context)
        {
            var routes = routingTable.GetRoutesFor(messageType);
            var selectedDestinations = SelectDestinationsForEachEndpoint(routes, context);
            return selectedDestinations.Select(destination => new UnicastRoutingStrategy(destination));
        }

        HashSet<string> SelectDestinationsForEachEndpoint(IEnumerable<UnicastRouteGroup> routeGroups, IOutgoingPublishContext context)
        {
            //Make sure we are sending only one to each transport destination. Might happen when there are multiple routing information sources.
            var addresses = new HashSet<string>();

            foreach (var group in routeGroups)
            {
                if (group.EndpointName == null) //Routing targets that do not specify endpoint name
                {
                    //Send a message to each target as we have no idea which endpoint they represent
                    foreach (var subscriber in group.Routes)
                    {
                        foreach (var address in ResolveRoute(subscriber))
                        {
                            addresses.Add(address);
                        }
                    }
                }
                else
                {
                    var candidates = group.Routes.SelectMany(ResolveRoute).ToArray();
                    var distributionStrategy = distributionPolicy.GetDistributionStrategy(@group.EndpointName, DistributionStrategyScope.Publish);
                    var distributionContext = new DistributionContext(candidates, context.Message, context.MessageId, context.Headers, resolveTransportAddress, context.Extensions);
                    var selected = distributionStrategy.SelectDestination(distributionContext);
                    addresses.Add(selected);
                }
            }

            return addresses;
        }

        IEnumerable<string> ResolveRoute(UnicastRoute route)
        {
            if (route.Instance != null)
            {
                yield return resolveTransportAddress(route.Instance);
            }
            else if (route.PhysicalAddress != null)
            {
                yield return route.PhysicalAddress;
            }
            else
            {
                foreach (var instance in endpointInstances.FindInstances(route.Endpoint))
                {
                    yield return resolveTransportAddress(instance);
                }
            }
        }
    }
}