using System;
using DotNet.Sdk.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly
{
    [Trait("Category", "Options")]
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

        public class SomeOptions
        {
            public string? Value { get; set; }
        }
    }
}
