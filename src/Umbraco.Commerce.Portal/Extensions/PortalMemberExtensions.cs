using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Extensions;

public static class PortalMemberExtensions
{
    public static AccountModel ToModel(this IMember member)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));

        return new AccountModel
        {
            Id = member.Key,
            Email = member.Email,
            FirstName = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName) ?? string.Empty,
            LastName = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName) ?? string.Empty,
            Country = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country) ?? string.Empty,
            Street = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street) ?? string.Empty,
            City = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City) ?? string.Empty,
            State = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State) ?? string.Empty,
            ZipCode = member.GetValue<string>(UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode) ?? string.Empty,
        };
    }
}
