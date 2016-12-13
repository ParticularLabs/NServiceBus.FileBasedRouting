using NServiceBus.Configuration.AdvanceExtensibility;

namespace NServiceBus.FileBasedRouting
{
    public static class FileBasedRoutingConfigExtensions
    {
        /// <summary>
        /// Enables routing configured with the routing configuration file.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        public static void EnableFileBasedRouting(this EndpointConfiguration config)
        {
            config.EnableFeature<FileBasedRoutingFeature>();
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFilePath"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFilePath">The path to the configuration file.</param>
        public static void EnableFileBasedRouting(this EndpointConfiguration config, string configurationFilePath)
        {
            config.GetSettings().Set(FileBasedRoutingFeature.RoutingFilePathKey, configurationFilePath);
            config.EnableFeature<FileBasedRoutingFeature>();
        }
    }
}