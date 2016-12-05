using System;
using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;

namespace EndpointC
{
    class Program
    {
        static void Main(string[] args)
        {
            Start().GetAwaiter().GetResult();
        }

        static async Task Start()
        {
            var endpointConfiguration = new EndpointConfiguration("endpointC");

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var routingConfig = endpointConfiguration.UseTransport<MsmqTransport>().Routing();
            routingConfig.RegisterPublisher(typeof(DemoEvent), "endpointA");

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
