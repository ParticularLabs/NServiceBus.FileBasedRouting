using System;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Events;
using NServiceBus;
using NServiceBus.FileBasedRouting;

class Program
{
    static async Task Main()
    {
        var endpointConfiguration = new EndpointConfiguration("endpointA");

        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");

        var routingConfig = endpointConfiguration.UseTransport<MsmqTransport>().Routing();
        routingConfig.RegisterPublisher(typeof(DemoCommandReceived), "endpointB");
        routingConfig.InstanceMappingFile().FilePath("instance-mapping.xml");
        routingConfig.UseFileBasedRouting();

        var endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press [c] to send a command. Press [e] to publish an event. Press [Esc] to quit.");

        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Escape)
            {
                break;
            }

            if (key.Key == ConsoleKey.C)
            {
                var commandId = Guid.NewGuid();
                var demoCommand = new DemoCommand
                {
                    CommandId = commandId
                };
                await endpoint.Send(demoCommand)
                    .ConfigureAwait(false);
                Console.WriteLine();
                Console.WriteLine("Sent command with id: " + commandId);
            }

            if (key.Key == ConsoleKey.E)
            {
                var eventId = Guid.NewGuid();
                var demoEvent = new DemoEvent
                {
                    EventId = eventId
                };
                await endpoint.Publish(demoEvent)
                    .ConfigureAwait(false);
                Console.WriteLine();
                Console.WriteLine("Sent event with id: " + eventId);
            }
        }

        await endpoint.Stop()
            .ConfigureAwait(false);
    }
}