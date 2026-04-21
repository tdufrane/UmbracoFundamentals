using Umbraco.Cms.Core.Strings;

namespace UmbracoFundamentals.Code
{
    public static class StringExtensions
    {
        //used for the Intro Property of the Recipe Document Type 
        public static IHtmlEncodedString ConvertLineBreaksToBrTags(this string input)
        {
            return new HtmlEncodedString(input.Replace("\n", "<br />"));
        }

        public static string ShortenTo100WithDots(this string input)
            => input.ShortenTo(100, "...");

        public static string ShortenTo(this string input, int amount, string ending)
        {
            if (input.Length <= 100)
                return input;

            return (input.Contains(' ')
                       ? input.Substring(0, input.Substring(0, amount - ending.Length).LastIndexOf(' '))
                       : input.Substring(0, amount - ending.Length))
                   + ending;
        }

        // this is used in exercise 2
        public static string? WithFallback(this string? input, string? fallback)
            => input.IsNullOrWhiteSpace() == false
                ? input
                : fallback;
    }
}
