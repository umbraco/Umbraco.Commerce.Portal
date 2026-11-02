using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Portal.Pipeline.Tasks;

/// <summary>
/// Publishes the portal pages this install created, so the portal works straight after install.
/// Runs last, after public access is set up, so the account pages are never live unprotected.
/// Pages that already existed are left alone, so a re-install doesn't publish an editor's changes.
/// </summary>
internal class PublishUmbracoCommercePortalNodesTask(
    IContentService contentService,
    ILogger<PublishUmbracoCommercePortalNodesTask> logger)
    : PipelineTaskBase<InstallPipelineContext>
{
    /// <inheritdoc />
    public override Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
    {
        if (args.Model.CreatedPortalContainerPageId is not int containerId
            || contentService.GetById(containerId) is not IContent portalContainerPage)
        {
            return Task.FromResult(this.Ok());
        }

        // Publishing fails when the site root isn't published. The pages stay created (unpublished)
        // and the editor can publish them once the site root is published.
        string[] failed = contentService
            .PublishBranch(portalContainerPage, PublishBranchFilter.IncludeUnpublished, ["*"])
            .Where(x => !x.Success)
            .Select(x => $"{x.Content?.Name} ({x.Result})")
            .ToArray();
        if (failed.Length > 0)
        {
            logger.LogWarning(
                "Umbraco Commerce Portal pages were created but not all could be published: {FailedPages}. Publish the Portal page and its descendants in the backoffice.",
                string.Join(", ", failed));
        }

        return Task.FromResult(this.Ok());
    }
}
