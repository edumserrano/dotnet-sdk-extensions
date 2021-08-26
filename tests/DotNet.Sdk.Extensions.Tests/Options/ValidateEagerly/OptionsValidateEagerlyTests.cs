using System;
using System.Diagnostics.CodeAnalysis;
using DotNet.Sdk.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly
{
    [Trait("Category", XUnitCategories.Options)]
    public class OptionsValidateEagerlyTests
    {
        /// <summary>
        /// Validates arguments for the <see cref="OptionsBuilderExtensions.ValidateEagerly{T}"/> extension method.
        /// </summary>
        [Fact]
        public void ValidatesArguments()
        {
            var optionsBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() =>
            {
                OptionsBuilderExtensions.ValidateEagerly<SomeOptions>(optionsBuilder: null!);
            });
            optionsBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'optionsBuilder')");
        }

        [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for Options types. Used as generic type param.")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class SomeOptions
        {
            public string? Value { get; set; }
        }
    }
}
