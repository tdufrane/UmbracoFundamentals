using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class RecipeAreaContentViewModel : SiteViewModel
    {
        public RecipeAreaContentViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
            Content = (RecipeArea)content;
        }
        public RecipeArea Content { get; }
    }
}
