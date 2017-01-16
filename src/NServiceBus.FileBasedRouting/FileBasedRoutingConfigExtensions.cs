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
        public static void UseFileBasedRouting(this RoutingSettings config)
        {
            config.GetSettings().EnableFeatureByDefault<FileBasedRoutingFeature>();
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFilePath"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFilePath">The path to the configuration file.</param>
        public static void UseFileBasedRouting(this RoutingSettings config, string configurationFilePath)
        {
            config.UseFileBasedRouting(UriHelper.FilePathToUri(configurationFilePath));
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFileUri"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFileUri">The <see cref="Uri"/> to the configuration file.</param>
        public static void UseFileBasedRouting(this RoutingSettings config, Uri configurationFileUri)
        {
            config.GetSettings().Set(FileBasedRoutingFeature.RoutingFilePathKey, configurationFileUri);
            config.GetSettings().EnableFeatureByDefault<FileBasedRoutingFeature>();
        }
    }
}