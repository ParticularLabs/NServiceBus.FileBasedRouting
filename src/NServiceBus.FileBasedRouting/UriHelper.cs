namespace NServiceBus.FileBasedRouting
{
    using System;
    using System.IO;

    class UriHelper
    {
        public static Uri FilePathToUri(string filePath)
        {
            var absoluteFilePath = Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            return new Uri(absoluteFilePath, UriKind.Absolute);
        }
    }
}