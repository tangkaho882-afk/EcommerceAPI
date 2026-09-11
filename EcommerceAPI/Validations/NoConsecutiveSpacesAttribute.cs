using EcommerceAPI.DTOs.User;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Validations
{
    public class NoConsecutiveSpacesAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
           if(value is not string text )
            {
                return ValidationResult.Success;
            }

           var dto = (RegisterDto)validationContext.ObjectInstance;
           

           if(!string.IsNullOrWhiteSpace(text)&&
                text.Contains("  ") )
            {
                return new ValidationResult($"{validationContext.DisplayName} cannot contain consecutive spaces.");
            }

            return ValidationResult.Success;
        }
    }
}
