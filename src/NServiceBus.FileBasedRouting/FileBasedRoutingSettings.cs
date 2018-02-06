namespace NServiceBus.FileBasedRouting
{
    using System;
    using Configuration.AdvanceExtensibility;

    /// <summary>
    /// File based routing configuration instance.
    /// </summary>
    public class FileBasedRoutingSettings
    {
        EndpointConfiguration endpointConfiguration;

        internal FileBasedRoutingSettings(EndpointConfiguration endpointConfiguration)
        {
            this.endpointConfiguration = endpointConfiguration;
        }

        /// <summary>
        /// Enables PubSub from code even when using file based routing.
        /// </summary>
        public void EnableMessageDrivenPubSub()
        {
            endpointConfiguration.GetSettings().Set(FileBasedRoutingFeature.MessageDrivenPubSubEnabled, true);
            //endpointConfiguration.EnableFeature<FileBasedRoutingFeature>();
        }
    }
}