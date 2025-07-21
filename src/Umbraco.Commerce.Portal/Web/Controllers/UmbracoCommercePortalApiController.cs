using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Installer = Umbraco.Commerce.Portal.Services.InstallService;

namespace Umbraco.Commerce.Portal.Web.Controllers;

[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute("umbraco-commerce-portal")]
[ApiExplorerSettings(GroupName = "Umbraco Commerce Portal API")]
[Authorize]
[Authorize(AuthorizationPolicies.SectionAccessSettings)]
public class UmbracoCommercePortalApiController(IUmbracoCommerceApi commerceApi, IContentService contentService)
    : ManagementApiControllerBase
{
    [HttpGet("install")]
    public async Task<object> InstallUmbracoCommercePortal(Guid? siteRootNodeId)
    {
        ArgumentNullException.ThrowIfNull(siteRootNodeId);

        // Validate the site root node
        IContent? siteRootNode = contentService.GetById(siteRootNodeId.Value);
        if (siteRootNode == null)
        {
            return new { success = false, message = "Couldn't find the site root node. Please check if the root you picked stills exists." };
        }

        Guid? storeId = GetStoreId(siteRootNode);
        if (!storeId.HasValue)
        {
            return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };
        }

        StoreReadOnly store = await commerceApi.GetStoreAsync(storeId.Value);
        if (store == null)
        {
            return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };
        }

        // Perform the install
        await new Installer().InstallAsync(siteRootNode.Id, store);

        return new { success = true };
    }

    private Guid? GetStoreId(IContent content)
    {
        if (content.HasProperty(Cms.Constants.Properties.StorePropertyAlias))
        {
            return content.GetValue<Guid?>(Cms.Constants.Properties.StorePropertyAlias);
        }

        if (content.ParentId != -1)
        {
            return GetStoreId(contentService.GetById(content.ParentId)!);
        }

        return null;
    }
}
