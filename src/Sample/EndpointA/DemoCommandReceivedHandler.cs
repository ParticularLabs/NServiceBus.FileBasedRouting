using System;
using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;

public class DemoCommandReceivedHandler : IHandleMessages<DemoCommandReceived>
{
    public Task Handle(DemoCommandReceived message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received {nameof(DemoCommandReceived)} for command {message.ReceivedCommandId}");
        return Task.FromResult(0);
    }
}