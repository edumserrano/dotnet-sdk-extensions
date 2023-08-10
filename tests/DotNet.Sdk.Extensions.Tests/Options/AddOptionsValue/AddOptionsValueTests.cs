using Microsoft.AspNetCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;

namespace DotNet.Sdk.Extensions.Tests.Options.AddOptionsValue;

[Trait("Category", XUnitCategories.Options)]
public class AddOptionsValueTests
{
    /// <summary>
    /// Tests that the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(IServiceCollection,IConfiguration)"/>
    /// extension method adds the type T option to the <see cref="IServiceCollection"/>.
    /// </summary>
    [Fact]
    public void AddsOptionsType1()
    {
        using var configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptionsValue<MyOptions>(configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myOptions = serviceProvider.GetService<MyOptions>();
        myOptions.ShouldNotBeNull();
    }

    /// <summary>
    /// Validates arguments for the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(IServiceCollection,IConfiguration)"/>
    /// extension method.
    /// </summary>
    [Fact]
    public void ValidatesArguments1()
    {
        using var configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
        var servicesArgumentNullException = Should.Throw<ArgumentNullException>(() =>
        {
            OptionsBuilderExtensions.AddOptionsValue<MyOptions>(services: null!, configuration);
        });
        servicesArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'services')");
    }

    /// <summary>
    /// Tests that the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(IServiceCollection,IConfiguration,string)"/>
    /// extension method adds the type T option to the <see cref="IServiceCollection"/> and maps to the correct section.
    /// </summary>
    [Fact]
    public void AddsOptionsType2()
    {
        var memoryConfigurationSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("MyOptionsSection:SomeOption", "some value"),
            },
        };
        var memoryConfigurationProvider = new MemoryConfigurationProvider(memoryConfigurationSource);
        var configurationProviders = new List<IConfigurationProvider> { memoryConfigurationProvider };
        using var configuration = new ConfigurationRoot(configurationProviders);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptionsValue<MyOptions>(configuration, sectionName: "MyOptionsSection");
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myOptions = serviceProvider.GetRequiredService<MyOptions>();
        myOptions.SomeOption.ShouldBe("some value");
    }

    /// <summary>
    /// Validates arguments for the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(IServiceCollection,IConfiguration,string)"/>
    /// extension method.
    /// </summary>
    [Fact]
    public void ValidatesArguments2()
    {
        using var configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
        var serviceCollection = new ServiceCollection();
        var servicesArgumentNullException = Should.Throw<ArgumentNullException>(() =>
        {
            OptionsBuilderExtensions.AddOptionsValue<MyOptions>(services: null!, configuration, sectionName: "MyOptionsSection");
        });
        servicesArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'services')");
        var configurationArgumentNullException = Should.Throw<ArgumentNullException>(() =>
        {
            serviceCollection.AddOptionsValue<MyOptions>(configuration: null!, sectionName: "MyOptionsSection");
        });
        configurationArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'configuration')");
    }

    /// <summary>
    /// Tests that the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(OptionsBuilder{T})"/>
    /// extension method adds the type T option to the <see cref="IServiceCollection"/>.
    /// </summary>
    [Fact]
    public void AddsOptionsType3()
    {
        using var configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddOptions<MyOptions>()
            .Bind(configuration)
            .AddOptionsValue();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myOptions = serviceProvider.GetService<MyOptions>();
        myOptions.ShouldNotBeNull();
    }

    /// <summary>
    /// Validates arguments for the <see cref="OptionsBuilderExtensions.AddOptionsValue{T}(OptionsBuilder{T})"/>
    /// extension method.
    /// </summary>
    [Fact]
    public void ValidatesArguments3()
    {
        var optionsBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() =>
        {
            OptionsBuilderExtensions.AddOptionsValue<MyOptions>(optionsBuilder: null!);
        });
        optionsBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'optionsBuilder')");
    }

    [Fact]
    public void AddsOptionsType4()
    {
        using var webHost = WebHost
            .CreateDefaultBuilder()
            .Configure((_, _) =>
            {
                // this is required just to provide a configuration for the webhost
                // or else it fails when calling webHostBuilder.Build()
            })
            .ConfigureAppConfiguration(config =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string?>>
                    {
                        new KeyValuePair<string, string?>("MyOptionsSection:SomeOption", "original-value"),
                    },
                };
                config.Add(memoryConfigurationSource);
            })
            .ConfigureServices((context, services) =>
            {
                var namedConfigSection = context.Configuration.GetSection("MyOptionsSection");
                services.Configure<MyOptions>(context.Configuration.GetSection("MyOptionsSection"));
            })
            .UseConfigurationValue("MyOptionsSection:SomeOption", "test-value")
            .Build();

        var myOptions = webHost.Services.GetService<IOptions<MyOptions>>();
        myOptions.ShouldNotBeNull();
    }

    private sealed class MyOptions
    {
        public string? SomeOption { get; set; }
    }
}
