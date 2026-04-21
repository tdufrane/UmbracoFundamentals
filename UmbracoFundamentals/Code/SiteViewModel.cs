using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Code
{
    public class SiteViewModel : PublishedContentWrapped
    {
        public SiteViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
            Site = new SiteContent(content);
        }
        public SiteContent Site { get; }
    }

    public class SiteContent
    {
        public SiteContent(IPublishedContent content)
        {
            var homePage = content.Root<HomePage>();
            Name = $"{content.Name} - {homePage?.CompanyName}";
            ColorGradient1 = homePage?.Value<string>("colorSettingsGradientStart") ?? "#1b264f";
            ColorGradient2 = homePage?.Value<string>("colorSettingsGradientEnd") ?? "#3e4c9f";
            ColorAccent1 = homePage?.Value<string>("colorSettingsAccent1") ?? "#e9b1aa";
            ColorAccent2 = homePage?.Value<string>("colorSettingsAccent2") ?? "#f5c0ba";
        }
        public string Name { get; }
        public string ColorGradient1 { get; }
        public string ColorGradient2 { get; }
        public string ColorAccent1 { get; }
        public string ColorAccent2 { get; } 
    }
}
