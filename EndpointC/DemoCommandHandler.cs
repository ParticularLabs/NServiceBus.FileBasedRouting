using System;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Events;
using NServiceBus;

namespace EndpointC
{
    class OtherCommandHandler : IHandleMessages<OtherCommand>
    {
        public Task Handle(OtherCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received {nameof(OtherCommand)} {message.CommandId}");
            return context.Publish(new DemoCommandReceived { ReceivedCommandId = message.CommandId });
        }
    }
}
