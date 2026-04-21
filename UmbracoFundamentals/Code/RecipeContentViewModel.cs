using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class RecipeContentViewModel : SiteViewModel
    {
        public RecipeContentViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
            Content = new RecipeViewModel((Recipe)content);
            Member = new MemberContent();
        }
        public RecipeViewModel Content { get; }
        public MemberContent Member { get; set; }
    }
    public class RecipeViewModel
    {
        // we could also mark the properties as [maybenull] like done on modelsbuilder instead of nullchecking and returning no null default values

        public RecipeViewModel(Recipe content)
        {
            Heading = content.Title.WithFallback(content.Name) ?? string.Empty;
            Intro = content.Intro?.ConvertLineBreaksToBrTags() ?? new HtmlEncodedString(string.Empty);
            Preparation = content.Preparation ?? new HtmlEncodedString(string.Empty);
        }

        public string Heading { get; }
        public IHtmlEncodedString Intro { get; }
        public IHtmlEncodedString Preparation { get; }
    }

    public class MemberContent
    {
        public bool IsLoggedIn { get; set; }
        public bool HasUpVoted { get; set; }
    }
}
