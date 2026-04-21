using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class TextPageContentViewModel : SiteViewModel
    {
        public TextPageContentViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
            Content = (TextPage)content;
        }
        public TextPage Content { get; }
    }
}
