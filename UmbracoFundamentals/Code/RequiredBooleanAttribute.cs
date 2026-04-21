using System.ComponentModel.DataAnnotations;

namespace UmbracoFundamentals.Code
{
    public class RequiredBooleanAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null && (bool)value == true;
        }
    }
}
