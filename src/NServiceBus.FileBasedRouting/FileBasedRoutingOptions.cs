namespace NServiceBus.FileBasedRouting
{
    using Settings;

    /// <summary>
    /// Provides configuration options for FileBasedRouting feature
    /// </summary>
    public class FileBasedRoutingOptions
    {
        SettingsHolder settings;

        /// <inheritdoc />
        public FileBasedRoutingOptions(SettingsHolder settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Enables Subscriptions from code even when using file based routing.
        /// </summary>
        public void EnableMessageDrivenSubscriptions()
        {
            settings.Set(FileBasedRoutingFeature.MessageDrivenSubscriptionsEnabled, true);
        }
    }
}