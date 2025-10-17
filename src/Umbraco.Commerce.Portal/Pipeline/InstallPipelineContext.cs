using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Portal.Pipeline;

public class InstallPipelineContext
{
    public int SiteRootNodeId { get; set; }

    public StoreReadOnly Store { get; set; }
}
