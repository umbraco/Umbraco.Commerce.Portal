using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Pipeline;
using PipelineRunner = Umbraco.Commerce.Common.Pipelines.Pipeline;

namespace Umbraco.Commerce.Portal.Services;

internal class InstallService
{
    public async Task InstallAsync(int siteRootNodeId, StoreReadOnly store)
    {
        PipelineResult<InstallPipelineContext> result = await PipelineRunner.ExecuteAsync<InstallAsyncPipelineTask, InstallPipelineArgs, InstallPipelineContext>(
            new InstallPipelineArgs(
                new InstallPipelineContext
                {
                    SiteRootNodeId = siteRootNodeId,
                    Store = store
                }),
            cancellationToken: default
        );

        if (!result.Success)
        {
            throw result.Exception;
        }
    }
}
