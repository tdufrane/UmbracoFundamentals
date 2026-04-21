using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.PublishedModels;
using UmbracoFundamentals.Services;

namespace UmbracoFundamentals.Controllers
{
    // for "simplicities sake", we will be storing up-votes in memory
    // this is not a feasible way of doing things in real life as when the server restarts, all up-votes are gone
    // and it is also not suited for multiple members up voting at the same time

    [ApiController]
    [UmbracoMemberAuthorize]
    public class RecipeUpvoteController : ControllerBase
    {
        public const string MemCacheKey = "Recipe_UpVote";
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMemberManager _memberManager;
        private readonly IRecipeUpVoteService _recipeUpVoteService;

        public RecipeUpvoteController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IMemberManager memberManager,
            IRecipeUpVoteService recipeUpVoteService)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _memberManager = memberManager;
            _recipeUpVoteService = recipeUpVoteService;
        }

        [HttpPost("/umbraco/member-api/recipe/{id:guid}/upvote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Upvote(Guid id)
        {
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            if (umbracoContext.Content is null)
            {
                return BadRequest("Could not establish an umbraco content context");
            }
            var recipe = umbracoContext.Content.GetById(id);

            // returning a separate message for invalid id's leaks internal information
            if (recipe is null || recipe.ContentType.Alias is not Recipe.ModelTypeAlias || await _memberManager.MemberHasAccessAsync(recipe.Path) == false)
            {
                return Unauthorized();
            }

            // attribute on the controller can only pass if there is a currentMember
            var memberKey = (await _memberManager.GetCurrentMemberAsync())!.Key;

            var upvoteResult = _recipeUpVoteService.UpVote(id, memberKey);
            if (upvoteResult == RecipeUpVoteResult.FailedAlreadyUpVotedByMember)
            {
                return BadRequest("Recipe was already up-voted by the currently logged in member");
            }

            return Ok();
        }
    }
}
