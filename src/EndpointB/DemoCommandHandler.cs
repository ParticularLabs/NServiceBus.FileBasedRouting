using System;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Events;
using NServiceBus;

namespace EndpointB
{
    class DemoCommandHandler : IHandleMessages<DemoCommand>
    {
        public Task Handle(DemoCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received {nameof(DemoCommand)} {message.CommandId}");
            return context.Publish(new DemoCommandReceived { ReceivedCommandId = message.CommandId });
        }
    }
}
