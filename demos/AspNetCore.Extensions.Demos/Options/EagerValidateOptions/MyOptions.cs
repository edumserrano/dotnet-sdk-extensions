using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Extensions.Demos.Options.EagerValidateOptions
{
    public class MyOptionsEager
    {
        [Required]
        public string SomeOption { get; set; }
    }
}
