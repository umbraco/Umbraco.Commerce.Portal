using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Portal.Pipeline;

public class InstallPipelineContext
{
    public int SiteRootNodeId { get; set; }

    public StoreReadOnly Store { get; set; }

    /// <summary>
    /// Gets or sets the id of the Portal page when this install created it, so the pages are published
    /// at the end of the install. Stays <c>null</c> when the pages already existed.
    /// </summary>
    internal int? CreatedPortalContainerPageId { get; set; }
}
