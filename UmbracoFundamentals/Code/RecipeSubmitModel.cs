
using System.ComponentModel.DataAnnotations;

namespace UmbracoFundamentals.Code
{
    public class RecipeSubmitModel
    {
        [Required]
        public string Name { get; set; }

        public string? Intro { get; set; }

        [Required]
        public string Preparation { get; set; }

        public IFormFile? ListImage { get; set; }
        //[RequiredBoolean(ErrorMessage = "Please confirm that all sources have been noted and no copyright laws have been broken.")]
        public bool SourceConfirmation { get; set; }
    }
}
