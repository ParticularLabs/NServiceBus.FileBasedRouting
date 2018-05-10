using System;

namespace NServiceBus.FileBasedRouting
{
    using Configuration.AdvancedExtensibility;
    using Features;

    /// <summary>
    /// Extensions to <see cref="RoutingSettings"/> to add file based routing functionality.
    /// </summary>
    public static class FileBasedRoutingConfigExtensions
    {
        /// <summary>
        /// Enables routing configured with the routing configuration file.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config)
        {
            var settings = config.GetSettings();
            settings.EnableFeatureByDefault<FileBasedRoutingFeature>();
            return new FileBasedRoutingOptions(settings);
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="updateInterval">The interval the route file should be checked for changes.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config, TimeSpan updateInterval)
        {
            if (updateInterval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update interval cannot be negative.");

            var settings = config.GetSettings();
            settings.Set(FileBasedRoutingFeature.RouteFileUpdateInterval, updateInterval);
            settings.EnableFeatureByDefault<FileBasedRoutingFeature>();
            return new FileBasedRoutingOptions(settings);
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFilePath"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFilePath">The path to the configuration file.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config, string configurationFilePath)
        {
            return config.UseFileBasedRouting(UriHelper.FilePathToUri(configurationFilePath));
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFilePath"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFilePath">The path to the configuration file.</param>
        /// <param name="updateInterval">The interval the route file should be checked for changes.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config, string configurationFilePath, TimeSpan updateInterval)
        {
            return config.UseFileBasedRouting(UriHelper.FilePathToUri(configurationFilePath), updateInterval);
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFileUri"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFileUri">The <see cref="Uri"/> to the configuration file.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config, Uri configurationFileUri)
        {
            var settings = config.GetSettings();
            settings.Set(FileBasedRoutingFeature.RoutingFilePathKey, configurationFileUri);
            settings.EnableFeatureByDefault<FileBasedRoutingFeature>();
            return new FileBasedRoutingOptions(settings);
        }

        /// <summary>
        /// Enables routing configured with the routing configuration file under <paramref name="configurationFileUri"/>
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="configurationFileUri">The <see cref="Uri"/> to the configuration file.</param>
        /// <param name="updateInterval">The interval the route file should be checked for changes.</param>
        public static FileBasedRoutingOptions UseFileBasedRouting(this RoutingSettings config, Uri configurationFileUri, TimeSpan updateInterval)
        {
            if (updateInterval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update interval cannot be negative.");

            var settings = config.GetSettings();
            settings.Set(FileBasedRoutingFeature.RouteFileUpdateInterval, updateInterval);
            settings.Set(FileBasedRoutingFeature.RoutingFilePathKey, configurationFileUri);
            settings.EnableFeatureByDefault<FileBasedRoutingFeature>();
            return new FileBasedRoutingOptions(settings);
        }
    }
}