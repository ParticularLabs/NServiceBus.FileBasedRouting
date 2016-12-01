using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

namespace NServiceBus.FileBasedRouting
{
    class UnsubscribeTerminator : PipelineTerminator<IUnsubscribeContext>
    {
        protected override Task Terminate(IUnsubscribeContext context)
        {
            log.Debug($"Unsubscribe was called for {context.EventType.FullName}. With FileBasedRouting, unsubscribe operations have no effect and subscribers should be configured in the routing file.");
            return Task.CompletedTask;
        }

        static readonly ILog log = LogManager.GetLogger<FileBasedRoutingFeature>();
    }
}