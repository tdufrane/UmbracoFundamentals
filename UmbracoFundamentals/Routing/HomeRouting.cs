using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Routing
{
    public class HomeRoutingComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            //builder.UrlProviders().InsertBefore<NewDefaultUrlProvider, HomeUrlProvider>();
            //builder.ContentFinders().InsertBefore<ContentFinderByUrlNew, HomeContentFinder>();
        }
    }

    public class HomeUrlProvider : NewDefaultUrlProvider
    {
        public HomeUrlProvider(IOptionsMonitor<RequestHandlerSettings> requestSettings, ILogger<NewDefaultUrlProvider> logger, ISiteDomainMapper siteDomainMapper, IUmbracoContextAccessor umbracoContextAccessor, UriUtility uriUtility, IPublishedContentCache publishedContentCache, IDomainCache domainCache, IIdKeyMap idKeyMap, IDocumentUrlService documentUrlService, IDocumentNavigationQueryService navigationQueryService, IPublishedContentStatusFilteringService publishedContentStatusFilteringService, ILanguageService languageService)
        : base(requestSettings, logger, siteDomainMapper, umbracoContextAccessor, uriUtility, publishedContentCache, domainCache, idKeyMap, documentUrlService, navigationQueryService, publishedContentStatusFilteringService, languageService)
        {
        }

        public override UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
        {
            // we can only overwrite if the page is a child of a site
            var siteNode = content.AncestorOrSelf<HomePage>();
            if (siteNode == null)
            {
                return null;
            }

            // get the configure homepage with the first child as fallback
            var selectedHome = siteNode.Home ?? siteNode.FirstChild();
            if (selectedHome?.Id != content.Id)
            {
                // we are not processing the selected homepage
                return null;
            }

            // return the url of the site which will be equal to the configured hostname for the given culture
            return base.GetUrl(siteNode, mode, culture, current);
        }
    }

    class HomeContentFinder : ContentFinderByUrlNew
    {
        public HomeContentFinder(ILogger<ContentFinderByUrlNew> logger, IUmbracoContextAccessor umbracoContextAccessor, IDocumentUrlService documentUrlService, IPublishedContentCache publishedContentCache) : base(logger, umbracoContextAccessor, documentUrlService, publishedContentCache)
        {
        }

        public override async Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
        {
            await base.TryFindContent(frequest);
            var baseResult = frequest.PublishedContent;
            if (baseResult is not HomePage siteNode)
            {
                return false;
            }

            if (siteNode.Home is not null)
            {
                // return the selected node
                frequest.SetPublishedContent(siteNode.Home);
                return true;
            }

            // take the first child as a fallback
            var firstChild = baseResult.FirstChild();
            if (firstChild == null)
            {
                return false;
            }

            frequest.SetPublishedContent(firstChild);
            return true;
        }
    }
}
