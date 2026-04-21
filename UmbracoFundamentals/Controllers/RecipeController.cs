using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using UmbracoFundamentals.Code;
using UmbracoFundamentals.Services;

namespace UmbracoFundamentals.Controllers
{
    public class RecipeController : RenderController
    {
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        private readonly IMemberManager _memberManager;
        private readonly IRecipeUpVoteService _recipeUpVoteService;
        public RecipeController(ILogger<RecipeController> logger, 
            ICompositeViewEngine compositeViewEngine, 
            IUmbracoContextAccessor umbracoContextAccessor,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext serviceContext,
            IMemberManager memberManager,
            IRecipeUpVoteService recipeUpVoteService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = serviceContext;
            _memberManager = memberManager;
            _recipeUpVoteService = recipeUpVoteService;
        }
        //...Constructor + injections go here
        public override IActionResult Index()
        {
            var recipeViewModel = new RecipeContentViewModel((Recipe)CurrentPage!,
            new PublishedValueFallback(_serviceContext, _variationContextAccessor));
            //exercise 5 addition
            // creating a model with anything more than simple mapping, should probably involve some kind of factory pattern
            // but for simplicity sake, we will do it in the controller
            if (_memberManager.IsLoggedIn())
            {
                recipeViewModel.Member.IsLoggedIn = true;

                var memberKey = _memberManager.GetCurrentMemberAsync().GetAwaiter().GetResult()!.Key;

                recipeViewModel.Member.HasUpVoted = _recipeUpVoteService.HasMemberUpVoted(CurrentPage!.Key, memberKey);
            }
            return CurrentTemplate(recipeViewModel);
        }
    }
}
