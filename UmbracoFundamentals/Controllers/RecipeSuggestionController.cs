using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;
using UmbracoFundamentals.Code;
using UmbracoFundamentals.Services;

namespace UmbracoFundamentals.Controllers
{
    public class RecipeSuggestionController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IRecipeUploadService _recipeUploadService;

        public RecipeSuggestionController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberManager memberManager,
            IRecipeUploadService recipeUploadService
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _recipeUploadService = recipeUploadService;
        }

        [HttpPost]
        [ValidateUmbracoFormRouteString] // make sure this form is only hit from within an umbraco route
                                         // we use the bind prefix here to match the property name we give to the asp input field
                                         // example: <input asp-for="@recipeSubmitModel.Name" class="form-control"/>
        public async Task<IActionResult> HandleSubmitRecipe([Bind(Prefix = "recipeSubmitModel")] RecipeSubmitModel model)
        {
            // in this exercise, the form is only rendered in protect pages so this check is not strictly necessary
            // but if you had a page where a form was conditionally rendered based on whether a member was logged in,
            // than this is how you could handle unauthorized submissions in case they bypassed the antiforgery token and conditional rendering.
            if (_memberManager.IsLoggedIn() == false)
            {
                // this should not be null according to our structure and the fact we are inside an umbraco based route (see [ValidateUmbracoFormRouteString])
                return RedirectToUmbracoPage(CurrentPage!.AncestorOrSelf<HomePage>()!.FirstChild<MemberError>()!);
            }

            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            TempData["FormSuccess"] = await _recipeUploadService.SaveRecipe(CurrentPage!, model);
            return RedirectToCurrentUmbracoPage();
        }
    }
}
