using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbracoFundamentals.Handlers
{
    public class MemberRecipePublishingNotificationHandler : INotificationHandler<ContentPublishingNotification>
    {
        private readonly IContentService _contentService;

        public MemberRecipePublishingNotificationHandler(IContentService contentService)
        {
            _contentService = contentService;
        }

        public void Handle(ContentPublishingNotification notification)
        {
            foreach (var entity in notification.PublishedEntities)
            {
                // we are only interest in recipes
                if (entity.ContentType.Alias is not Recipe.ModelTypeAlias)
                {
                    continue;
                }

                // we are only interest in recipes that have been submitted by members
                var parentEntity = _contentService.GetParent(entity);
                if (parentEntity!.ContentType.Alias is not MemberRecipeArea.ModelTypeAlias)
                {
                    continue;
                }

                var mainRecipeArea = _contentService.GetParent(parentEntity);
                entity.ParentId = mainRecipeArea!.Id;
            }
        }
    }
}
