using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class BlogEntryContentViewModel : SiteViewModel
    {
        public BlogEntryContentViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
            Content = (BlogEntry)content;
        }
        public BlogEntry Content { get; }
    }
}
