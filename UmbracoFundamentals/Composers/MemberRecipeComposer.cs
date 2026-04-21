using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using UmbracoFundamentals.Handlers;
using UmbracoFundamentals.Services;

namespace UmbracoFundamentals.Composers
{
    public class MemberRecipeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<IRecipeUploadService, RecipeUploadService>();
            builder.Services.AddScoped<IRecipeUpVoteService, RecipeUpVoteService>();
            builder.AddNotificationHandler<ContentPublishingNotification, MemberRecipePublishingNotificationHandler>();
        }
    }
}
