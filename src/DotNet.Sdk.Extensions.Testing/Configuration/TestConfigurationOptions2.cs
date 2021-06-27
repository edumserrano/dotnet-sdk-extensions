namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    /// <summary>
    /// Options to configure the appsettings when using <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(Microsoft.AspNetCore.Hosting.IWebHostBuilder,string,string[])"/>
    /// </summary>
    public class TestConfigurationOptions2
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