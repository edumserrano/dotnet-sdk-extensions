namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    public class TestConfigurationOptions
    {
        /// <summary>
        /// Directory for appsettings files.
        /// </summary>
        /// <remarks>
        /// It must be a relative directory to the current directory. It defaults to AppSettings.
        /// </remarks>
        public string AppSettingsDir { get; set; } = "AppSettings";
    }
}