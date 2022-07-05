using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.IValidateOptions;

public class MyOptions1Validation : IValidateOptions<MyOptions1>
{
    public ValidateOptionsResult Validate(string name, MyOptions1 options)
    {
        // this is doing data annotation validation but you can implement the validation however you like
        Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
        return ValidateOptionsResult.Success;
    }
}
