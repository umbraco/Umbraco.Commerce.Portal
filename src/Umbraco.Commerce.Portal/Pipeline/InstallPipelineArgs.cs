using Umbraco.Commerce.Common.Pipelines;

namespace Umbraco.Commerce.Portal.Pipeline;

internal class InstallPipelineArgs : PipelineArgs<InstallPipelineContext>
{
    public InstallPipelineArgs(InstallPipelineContext model)
        : base(model)
    {
    }

    public InstallPipelineContext InstallContext => Model;
}
