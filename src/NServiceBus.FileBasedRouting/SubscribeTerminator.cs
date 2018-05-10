using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

namespace NServiceBus.FileBasedRouting
{
    class SubscribeTerminator : PipelineTerminator<ISubscribeContext>
    {
        protected override Task Terminate(ISubscribeContext context)
        {
            log.Debug($"Subscribe was called for {context.EventType.FullName}. With FileBasedRouting, subscribe operations have no effect and subscribers should be configured in the routing file. If subscribe was not called by you, consider disabling the {nameof(AutoSubscribe)} feature.");
            return Task.FromResult(0);
        }

        static readonly ILog log = LogManager.GetLogger<FileBasedRoutingFeature>();
    }
}