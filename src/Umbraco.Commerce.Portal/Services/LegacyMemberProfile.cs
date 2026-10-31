using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Commerce.Portal.Services;

/// <summary>
/// The profile values Portal stored on the <c>ucpMember</c> member type before it moved onto
/// Commerce customers. Read once, when a member's customer is first set up, and never written.
/// </summary>
internal sealed record LegacyMemberProfile(
    string FirstName,
    string LastName,
    string Country,
    string Street,
    string City,
    string State,
    string ZipCode)
{
    public bool HasName => !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName);

    public bool HasAddress =>
        !string.IsNullOrWhiteSpace(Country)
        || !string.IsNullOrWhiteSpace(Street)
        || !string.IsNullOrWhiteSpace(City)
        || !string.IsNullOrWhiteSpace(ZipCode);

    public static LegacyMemberProfile FromMember(IMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        return new LegacyMemberProfile(
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State),
            Read(member, UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode));
    }

    // Members of other types don't have these properties; GetValue returns null for them.
    private static string Read(IMember member, string alias)
        => member.GetValue<string>(alias)?.Trim() ?? string.Empty;
}
