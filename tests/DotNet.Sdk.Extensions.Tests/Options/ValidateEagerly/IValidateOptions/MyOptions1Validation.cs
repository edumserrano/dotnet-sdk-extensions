using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.IValidateOptions
{
    public class MyOptions1Validation : IValidateOptions<MyOptions1>
    {
        public ValidateOptionsResult Validate(string name, MyOptions1 options1)
        {
            //this is doing data annotation validation but you can implement the validation however you like
            Validator.ValidateObject(options1, new ValidationContext(options1), validateAllProperties: true);
            return ValidateOptionsResult.Success;
        }
    }
}
