using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.FileBasedRouting;
using NServiceBus.Persistence;

namespace EndpointB
{
    static class Configuration
    {
        public static async Task Start(string discriminator)
        {
            var endpointConfiguration = new EndpointConfiguration("endpointB");
            endpointConfiguration.MakeInstanceUniquelyAddressable(discriminator);

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var routingConfig = endpointConfiguration.UseTransport<MsmqTransport>().Routing();
            routingConfig.RegisterPublisher(typeof(DemoEvent), "endpointA");

            //not used yet
            //endpointConfiguration.EnableFeature<FileBasedRoutingFeature>();

            var endpoint = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine("Press [Esc] to quit.");

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }

            await endpoint.Stop();
        }
    }
}
