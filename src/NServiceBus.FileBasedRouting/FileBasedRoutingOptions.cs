namespace NServiceBus.FileBasedRouting
{
    using Settings;

    /// <summary>
    /// Provides configuration options for FileBasedRouting feature
    /// </summary>
    public class FileBasedRoutingOptions
    {
        SettingsHolder settings;
        public FileBasedRoutingOptions(SettingsHolder settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Enables PubSub from code even when using file based routing.
        /// </summary>
        public void EnableMessageDrivenPubSub()
        {
            settings.Set(FileBasedRoutingFeature.MessageDrivenPubSubEnabled, true);
        }
    }
}