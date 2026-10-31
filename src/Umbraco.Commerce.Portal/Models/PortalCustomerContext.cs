using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Portal.Models;

/// <summary>
/// The signed-in member and their Umbraco Commerce customer for the current store.
/// </summary>
/// <param name="Member">The signed-in member.</param>
/// <param name="Customer">The member's customer in the current store.</param>
public sealed record PortalCustomerContext(IMember Member, CustomerReadOnly Customer);
