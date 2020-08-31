namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    public class TestConfigurationOptions
    {
        /// <summary>
        /// Path for the directory of the appsettings files.
        /// </summary>
        /// <remarks>
        /// <see cref="IsRelative"/> determines if this is a relative or absolute path.
        /// Defaults to AppSettings.
        /// </remarks>
        public string AppSettingsDir { get; set; } = "AppSettings";

        /// <summary>
        /// Defines if the <see cref="AppSettingsDir"/> is a relative or absolute path.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool IsRelative { get; set; } = true;
    }
}