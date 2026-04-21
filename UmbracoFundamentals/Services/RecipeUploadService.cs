using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;
using UmbracoFundamentals.Code;

namespace UmbracoFundamentals.Services
{
    public interface IRecipeUploadService
    {
        Task<bool> SaveRecipe(IPublishedContent currentPage, RecipeSubmitModel model);
    }
    public class RecipeUploadService : IRecipeUploadService
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentEditingService _contentEditingService;
        private readonly IUserIdKeyResolver _userIdKeyResolver;
        private readonly IPublishedContentTypeCache _publishedContentTypeCache;
        private readonly ITemporaryFileService _temporaryFileService;
        private readonly IMediaEditingService _mediaEditingService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IContentService _contentService;

        public RecipeUploadService(IContentTypeService contentTypeService, IContentEditingService contentEditingService, IUserIdKeyResolver userIdKeyResolver, IPublishedContentTypeCache publishedContentTypeCache, ITemporaryFileService temporaryFileService, IMediaEditingService mediaEditingService, IJsonSerializer jsonSerializer, IMediaTypeService mediaTypeService, IContentService contentService)
        {
            _contentTypeService = contentTypeService;
            _contentEditingService = contentEditingService;
            _userIdKeyResolver = userIdKeyResolver;
            _publishedContentTypeCache = publishedContentTypeCache;
            _temporaryFileService = temporaryFileService;
            _mediaEditingService = mediaEditingService;
            _jsonSerializer = jsonSerializer;
            _mediaTypeService = mediaTypeService;
            _contentService = contentService;
        }

        public async Task<bool> SaveRecipe(IPublishedContent currentPage, RecipeSubmitModel model)
        {
            var memberRecipeArea =
                currentPage?.AncestorOrSelf<HomePage>()?.FirstChild<RecipeArea>()?.FirstChild<MemberRecipeArea>();
            if (memberRecipeArea is null)
            {
                // we throw an error here because this should not happen if the backoffice is configured correctly
                throw new Exception("Could not determine MemberRecipeArea");
            }

            var recipeModelTypeKey = _contentTypeService.Get(Recipe.ModelTypeAlias)?.Key;
            if (recipeModelTypeKey is null)
            {
                // we throw an error here because this should not happen if the backoffice is configured correctly
                throw new Exception("Could not determine recipeModelTypeKey");
            }

            // for now the user picker stores an INT, but we need a GUID, Umbraco supplies a resolver for this
            var userKey = await _userIdKeyResolver.GetAsync(memberRecipeArea.SubmittingUser);

            var contentCreateModel = new ContentCreateModel
            {
                ContentTypeKey = recipeModelTypeKey.Value,
                ParentKey = memberRecipeArea.Key,
                Variants = new[] { new VariantModel { Name = model.Name } },
                Properties = new List<PropertyValueModel>
            {
                new PropertyValueModel
                {
                    // by using GetModelPropertyType instead of magic strings,
                    // we ensure the compiler will throw an error if the alias (and ths the c# property name) of the generated models changes
                    // or the property is deleted.
                    Alias = Recipe.GetModelPropertyType(_publishedContentTypeCache, r => r.Intro)!.Alias,
                    Value = model.Intro
                },
                new PropertyValueModel
                {
                    Alias = Recipe.GetModelPropertyType(_publishedContentTypeCache, r => r.Preparation)!.Alias,
                    Value = model.Preparation
                }
            }
            };

            // using the contentEditingService will run the data trough the propertyValueEditors, validating the data
            // if you would like a more brute force approach, use the contentService instead.
            var createRecipeAttempt = await _contentEditingService.CreateAsync(contentCreateModel,
                userKey
            );

            if (createRecipeAttempt.Success is false || createRecipeAttempt.Result.Content is null)
            {
                return false;
            }

            if (model.ListImage is null)
            {
                return true;
            }
            // we just fire this off and hope for the best, ideally one would split this up into 2 atomic operations
            // so create the item first. Then save the image and update the item on success.
            // or wrap it all into one big scope/transaction and fail everything if one part fails.
            await SaveListImage(model.ListImage, userKey, memberRecipeArea.SubmittingUser, createRecipeAttempt.Result.Content);

            return true;
        }

        private async Task SaveListImage(IFormFile image, Guid userKey, int userId, IContent savedRecipe)
        {
            var imageModelTypeKey = _mediaTypeService.Get(Image.ModelTypeAlias)?.Key;
            if (imageModelTypeKey is null)
            {
                // we throw an error here because this should not happen if the backoffice is configured correctly
                throw new Exception("Could not determine recipeModelTypeKey");
            }

            var tempFileKey = Guid.NewGuid();
            var tempFileCreateAttempt = await _temporaryFileService.CreateAsync(new CreateTemporaryFileModel
            {
                FileName = image.FileName,
                Key = tempFileKey,
                OpenReadStream = () => image.OpenReadStream()
            });

            if (tempFileCreateAttempt.Success is false)
            {
                return;
            }

            // bonus, put a media folder picker on the memberRecipeArea to decide where member uploaded images go
            var listImageCreateAttempt = await _mediaEditingService.CreateAsync(new MediaCreateModel
            {
                ContentTypeKey = imageModelTypeKey.Value,
                ParentKey = null,
                Variants = new[] { new VariantModel { Name = image.FileName } },
                Properties = new List<PropertyValueModel>
                {
                    new PropertyValueModel
                    {
                        Alias = Image.GetModelPropertyType(_publishedContentTypeCache, r => r.UmbracoFile)!.Alias,
                        Value = _jsonSerializer.Serialize(new ImageCropperValue{TemporaryFileId = tempFileKey})
                    },
                }
            },
                userKey);

            if (listImageCreateAttempt.Success is false || listImageCreateAttempt.Result.Content is null)
            {
                return;
            }

            // use the content service to update the value of the property without any form of validation
            // this is not the recommended approach, but sometimes you just want to do it quick and dirty
            // use the IContentEditingService.UpdateAsync() method if you want to update with validation. Slower, but safer.
            savedRecipe.Properties.First(p => p.Alias == Recipe.GetModelPropertyType(_publishedContentTypeCache, r => r.ListImage)!.Alias)
                .SetValue($"[{{\"key\":\"{Guid.NewGuid()}\",\"mediaKey\":\"{listImageCreateAttempt.Result.Content.Key}\",\"mediaTypeAlias\":\"\",\"crops\":[],\"focalPoint\":null}}]");

            _contentService.Save(savedRecipe, userId);
        }
    }
}
