using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

internal class CreateUmbracoCommercePortalNodesTask(
    IScopeProvider scopeProvider,
    IContentTypeService contentTypeService,
    IContentService contentService)
    : PipelineTaskBase<InstallPipelineContext>
{
    public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
    {
        using IScope scope = scopeProvider.CreateScope();

        IContentType ucpPortalContainerPage = contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage)
            ?? throw new InvalidOperationException("Portal Page Document Type is not found");
        IContentType ucpPortalAuthPage = contentTypeService.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage)
            ?? throw new InvalidOperationException("Portal Auth Page Document Type is not found");

        // Check if nodes exist
        IQuery<IContent> filter = scope.SqlContext.Query<IContent>().Where(x => x.ContentTypeId == ucpPortalContainerPage.Id);
        IEnumerable<IContent> childNodes = contentService.GetPagedChildren(args.Model.SiteRootNodeId, 1, 1, out long totalRecords, filter);

        if (totalRecords == 0)
        {
            // Create the container page
            IContent portalContainerPage = contentService.CreateAndSave(
                "Portal",
                args.Model.SiteRootNodeId,
                UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage);

            // Create the portal auth pages
            CreatePortalAuthPage(portalContainerPage, "Login", "Login");
            CreatePortalAuthPage(portalContainerPage, "Register", "Register");
            CreatePortalAuthPage(portalContainerPage, "Reset Password", "Reset Password");
        }

        scope.Complete();

        return Ok();
    }

    private void CreatePortalAuthPage(IContent parent, string name, string shortName)
    {
        IContent portalAuthNode = contentService.Create(
            name,
            parent.Id,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage);
        contentService.Save(portalAuthNode);
    }
}
