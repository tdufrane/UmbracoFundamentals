using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class HomePageContentViewModel : SiteViewModel
    {
        public HomePageContentViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
            Content = (HomePage)content;
        }
        public HomePage Content { get; }
    }
}
