using NServiceBus.Configuration.AdvanceExtensibility;
using System;

namespace NServiceBus.FileBasedRouting
{
    using Features;

    public static class FileBasedRoutingConfigExtensions
    {
        /// <summary>
        /// Enables routing configured with the routing configuration file.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="monitor">Should the route configuration file be reloaded for changed.</param>
        public static void UseFileBasedRouting(this RoutingSettings config, bool monitor = true)
        {
            config.GetSettings().Set(FileBasedRoutingFeature.MonitorRouteFile, monitor);
            config.GetSettings().EnableFeatureByDefault<FileBasedRoutingFeature>();
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFilePath"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFilePath">The path to the configuration file.</param>
        /// <param name="monitor">Should the route configuration file be reloaded for changed.</param>
        public static void UseFileBasedRouting(this RoutingSettings config, string configurationFilePath, bool monitor = true)
        {
            config.UseFileBasedRouting(UriHelper.FilePathToUri(configurationFilePath), monitor);
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFileUri"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFileUri">The <see cref="Uri"/> to the configuration file.</param>
        /// <param name="monitor">Should the route configuration file be reloaded for changed.</param>
        public static void UseFileBasedRouting(this RoutingSettings config, Uri configurationFileUri, bool monitor = true)
        {
            config.GetSettings().Set(FileBasedRoutingFeature.RoutingFilePathKey, configurationFileUri);
            config.GetSettings().Set(FileBasedRoutingFeature.MonitorRouteFile, monitor);
            config.GetSettings().EnableFeatureByDefault<FileBasedRoutingFeature>();
        }
    }
}