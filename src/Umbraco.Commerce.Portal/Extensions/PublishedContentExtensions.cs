using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Exceptions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Extensions;

internal static class PublishedContentExtensions
{
    public static IPublishedContent GetPortalContainerPage(this IPublishedContent content)
    {
        return content.AncestorOrSelf(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage)!;
    }

    public static IPublishedContent? GetLoginPage(this IPublishedContent content) =>
        content.Siblings()!
            .FirstOrDefault(x => x.ContentType.Alias == UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage && x.Name == "Login");

    public static IPublishedContent? GetRegisterPage(this IPublishedContent content) =>
        content.Siblings()!
            .FirstOrDefault(x => x.ContentType.Alias == UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage && x.Name == "Register");

    public static IPublishedContent? GetResetPasswordPage(this IPublishedContent content) =>
        content.Siblings()!
            .FirstOrDefault(x => x.ContentType.Alias == UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage && x.Name == "Reset Password");

    public static StoreReadOnly GetStore(this IPublishedContent content)
    {
        return content.Value<StoreReadOnly>(Cms.Constants.Properties.StorePropertyAlias, fallback: Fallback.ToAncestors)
            ?? throw new StoreDataNotFoundException();
    }

    public static string GetThemeColor(this IPublishedContent content)
    {
        // Check if the checkout page has a custom theme color set
        string? themeColor = GetPortalContainerPage(content).Value<string>("ucpThemeColor")?.TrimStart('#');
        if (!string.IsNullOrWhiteSpace(themeColor))
        {
            return themeColor;
        }

        // If not, fallback to the store's theme color, or black if that's not set
        var store = content.GetStore();
        var storeThemeColor = store.ThemeColor;
        return !string.IsNullOrWhiteSpace(storeThemeColor)
            ? storeThemeColor
            : "#000000";
    }

    public static IPublishedContent? GetPublishedContentPropertyByAlias(this IPublishedContent content, string alias) =>
        GetPortalContainerPage(content)
            .Value<IPublishedContent>(alias);
}
