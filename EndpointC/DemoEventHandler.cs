using System;
using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;

namespace EndpointC
{
    class DemoEventHandler : IHandleMessages<DemoEvent>
    {
        public Task Handle(DemoEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received {nameof(DemoEvent)} {message.EventId}");
            return Task.CompletedTask;
        }
    }
}
