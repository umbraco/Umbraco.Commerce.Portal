using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Portal.Extensions;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

internal class ConfigureUmbracoCommercePortalPublicAccessTask(
    IPublicAccessService publicAccessService,
    IContentTypeService contentTypeService,
    IContentService contentService)
    : PipelineTaskBase<InstallPipelineContext>
{
    public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
    {
        // Get portal container page
        var containerContentType = contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage);
        var portalContainerPage = contentService
            .GetPagedOfType(containerContentType.Id, 0, 10, out long _, null)
            .FirstOrDefault(x => x.ParentId == args.Model.SiteRootNodeId);
        if (portalContainerPage is null)
        {
            throw new InvalidOperationException("Portal Container Node is not found");
        }

        // Get portal management pages
        var managementContentType = contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage);
        var portalManagementPage = contentService.GetPagedOfType(managementContentType.Id, 0, 10, out long _, null)
            .FirstOrDefault(x => x.ParentId == portalContainerPage.Id);
        if (portalManagementPage is null)
        {
            throw new InvalidOperationException("Portal Management Node is not found");
        }

        var authContentType = contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage);
        var loginPage = contentService
            .GetPagedOfType(authContentType.Id, 0, 10, out long _, null)
            .FirstOrDefault(x =>
                x.Name == UmbracoCommercePortalConstants.ContentNodes.PortalAuthLoginPageNodeName
                && x.ParentId == portalContainerPage.Id);
        if (loginPage is null)
        {
            throw new InvalidOperationException("Portal Login Node is not found");
        }

        var entry = publicAccessService.GetEntryForContent(portalManagementPage);
        if (entry is null)
        {
            entry = new PublicAccessEntry(portalManagementPage, loginPage, loginPage, []);
            entry.AddRule(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup, "MemberRole");
        }

        publicAccessService.Save(entry);

        return Ok();
    }
}
