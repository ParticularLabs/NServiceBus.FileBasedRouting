using System;
using NServiceBus;

namespace Contracts.Commands
{
    public class OtherCommand : ICommand
    {
        public Guid CommandId { get; set; }
    }
}