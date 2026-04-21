using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace UmbracoFundamentals.Controllers;

//Necessary code for the new API to show in the Swagger documentation and Swagger UI
public class CustomApiEndpoints : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => "my-api-v1";
}

public class MyConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("my-api-v1", new OpenApiInfo { Title = "My API v1", Version = "1.0" });
        options.OperationFilter<CustomApiEndpoints>();
    }
}

public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.Services.ConfigureOptions<MyConfigureSwaggerGenOptions>();
}

//Creating the Controller
[ApiController]
[ApiVersion("1.0")]
[MapToApi("my-api-v1")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[JsonOptionsName(Constants.JsonOptionsNames.BackOffice)]
[Route("api/v{version:apiVersion}/")]
public class MyApiController : Controller
{
    private IUserService _userService;
    private IContentService _contentService;
    private IAuditService _auditService;


    public IEnumerable<IUser>? allUsers;
    public IEnumerable<IContent>? allContent;

    public MyApiController(IUserService userService, IContentService contentService, IAuditService auditService)
    {
        _userService = userService;
        _contentService = contentService;
        _auditService = auditService;
    }

    [HttpGet("recently-edited-items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(List<UserAuditData>), StatusCodes.Status200OK)]
    public IActionResult GetRecentlyEditedItems()
    {
        List<UserAuditData> ListOfUserAuditData = new List<UserAuditData>();
        long amountOfUsers;
        IEnumerable<IUser> allUsers = _userService.GetAll(0, 100, out amountOfUsers);
        List<string> userIds = new List<string>();

        List<string> lastEditedDates = new List<string>();

        foreach (var user in allUsers)
        {
            UserAuditData newUser = new UserAuditData();
            newUser.UserId = user.Key.ToString();
            IEnumerable<IAuditItem> auditItems = _auditService.GetUserLogs(user.Id, AuditType.Save);
            IEnumerable<IAuditItem> orderedAuditItems = auditItems.Where(auditItem => auditItem.EntityType == "Document").OrderByDescending(auditItem => auditItem.CreateDate).Take(1);
            string lastEditedContentIdString = "";
            string lastEditedDateString = "";
            if (orderedAuditItems.Any())
            {
                int lastEditedContentId = orderedAuditItems.First().Id;
                IContent? lastEditedContent = _contentService.GetById(lastEditedContentId);
                if (lastEditedContent != null)
                {
                    lastEditedContentIdString = lastEditedContent.Key.ToString();
                    lastEditedDateString = orderedAuditItems.First().CreateDate.ToString();
                }
            }
            else
            {
                lastEditedContentIdString = "No Edits";
                lastEditedDateString = "No Edit Date";
            }
            newUser.LastEditedContentId = lastEditedContentIdString;
            newUser.LastEditedDate = lastEditedDateString;
            ListOfUserAuditData.Add(newUser);
        }
        return Json(ListOfUserAuditData);
    }

    public class UserAuditData
    {
        public string? UserId { get; set; }
        public string? LastEditedContentId { get; set; }
        public string? LastEditedDate { get; set; }
    }
}