using System;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Events;
using NServiceBus;

class DemoCommandHandler : IHandleMessages<DemoCommand>
{
    public Task Handle(DemoCommand message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received {nameof(DemoCommand)} {message.CommandId}");
        var commandReceived = new DemoCommandReceived
        {
            ReceivedCommandId = message.CommandId
        };
        return context.Publish(commandReceived);
    }
}
