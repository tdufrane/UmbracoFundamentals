using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using UmbracoFundamentals.Code;

namespace UmbracoFundamentals.Controllers
{
    public class HomePageController : RenderController
    {
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public HomePageController(ILogger<HomePageController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext serviceContext)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = serviceContext;
        }

        public override IActionResult Index()
        {
            var viewModel = new HomePageContentViewModel((HomePage)CurrentPage!,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor));
            return CurrentTemplate(viewModel);
        }
    }
}
