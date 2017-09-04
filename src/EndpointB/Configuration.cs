using System;
using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;
using NServiceBus.FileBasedRouting;

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
        routingConfig.UseFileBasedRouting();

        var endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press [Esc] to quit.");

        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Escape)
            {
                break;
            }
        }

        await endpoint.Stop()
            .ConfigureAwait(false);
    }
}