namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.IValidateOptions;

public class MyOptions1
{
    [Required]
    public string SomeOption { get; set; } = default!;
}
