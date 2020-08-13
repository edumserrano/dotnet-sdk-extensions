using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Demos.Options.EagerValidateOptions
{
    public class MyOptionsEager
    {
        [Required] 
        public string SomeOption { get; set; } = default!;
    }
}
