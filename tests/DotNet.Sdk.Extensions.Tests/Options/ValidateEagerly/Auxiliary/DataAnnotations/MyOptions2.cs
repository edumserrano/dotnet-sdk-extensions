using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.DataAnnotations
{
    public class MyOptions2
    {
        [Required]
        public string SomeOption { get; set; } = default!;
    }
}
