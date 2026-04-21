using Microsoft.Extensions.Caching.Memory;
using UmbracoFundamentals.Code;

namespace UmbracoFundamentals.Services
{
    public interface IRecipeUpVoteService
    {
        RecipeUpVoteResult UpVote(Guid recipeKey, Guid memberKey);
        bool HasMemberUpVoted(Guid recipeKey, Guid memberKey);
    }

    // for "simplicities sake", we will be storing up-votes in memory
    // this is not a feasible way of doing things in real life as when the server restarts, all up-votes are gone
    // and it is also not suited for multiple members up voting at the same time

    public class RecipeUpVoteService : IRecipeUpVoteService
    {
        private const string MemCacheKey = "Recipe_UpVote";

        private readonly IMemoryCache _memoryCache;

        public RecipeUpVoteService(
            IMemoryCache memoryCache
        )
        {
            _memoryCache = memoryCache;
        }

        public RecipeUpVoteResult UpVote(Guid recipeKey, Guid memberKey)
        {
            _memoryCache.TryGetValue(MemCacheKey, out List<RecipeUpVote>? upVotes);
            if (upVotes is null)
            {
                upVotes = new List<RecipeUpVote>();
            }

            if (upVotes.Any(v => v.RecipeId == recipeKey && v.MemberId == memberKey))
            {
                return RecipeUpVoteResult.FailedAlreadyUpVotedByMember;
            }

            upVotes.Add(new RecipeUpVote
            {
                DateTime = DateTime.Now,
                MemberId = memberKey,
                RecipeId = recipeKey
            });
            _memoryCache.Set(MemCacheKey, upVotes);

            return RecipeUpVoteResult.Success;
        }

        public bool HasMemberUpVoted(Guid recipeKey, Guid memberKey)
        {
            _memoryCache.TryGetValue(MemCacheKey, out List<RecipeUpVote>? votes);
            return votes?.Any(v => v.MemberId == memberKey && v.RecipeId == recipeKey) is true;
        }
    }

    public enum RecipeUpVoteResult
    {
        Success,
        FailedAlreadyUpVotedByMember
    }
}
