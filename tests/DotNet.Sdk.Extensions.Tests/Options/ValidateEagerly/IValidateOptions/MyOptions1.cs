using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.IValidateOptions
{
    public class MyOptions1
    {
        [Required]
        public string SomeOption { get; set; } = default!;
    }
}
