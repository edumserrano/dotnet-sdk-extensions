using System.ComponentModel.DataAnnotations;

namespace EagerOptionsValidation
{
    public class MyOptionsEager
    {
        [Required] 
        public string SomeOption { get; set; } = default!;
    }
}
