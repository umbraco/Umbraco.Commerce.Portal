using System;
using System.Linq;
using System.Threading;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Exceptions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Portal.Extensions;

internal static class PublishedContentExtensions
{
    public static IPublishedContent? GetPortalContainerPage(this IPublishedContent content)
        => content.AncestorOrSelf(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage)!;

    public static IPublishedContent? GetPortalContainerPage(this IPublishedContent content, string contentTypeAlias, string? name)
    {
        var portalContainerPage = content.GetPortalContainerPage();

        // Attempt to lookup page in the descendants of the portal container page
        var authPage = portalContainerPage
            .Descendants()
            .FirstOrDefault(x => x.ContentType.Alias == contentTypeAlias && x.Name == name);
        if (authPage != null)
        {
            return authPage;
        }

        // Attempt to lookup page in the descendants of the portal management page. In this case, the content type alias is enough
        var portalManagementPage = portalContainerPage
            .Descendants()
            .FirstOrDefault(x => x.ContentType.Alias == UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage);
        var accountPage = portalManagementPage
               .Descendants()
               .FirstOrDefault(x => x.ContentType.Alias == contentTypeAlias);

        return accountPage;
    }

    public static IPublishedContent? GetPortalContainerSettingByAlias(this IPublishedContent content, string alias) =>
        content.AncestorOrSelf(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage)!.Value<IPublishedContent>(alias);

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

    public static string GetStoreLogo(this IPublishedContent content, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(content);

        var portalContainerPage = content.GetPortalContainerPage();
        if (portalContainerPage == null)
        {
            return string.Empty;
        }

        return portalContainerPage.HasValue("ucpStoreLogo")
            ? portalContainerPage.Value<IPublishedContent>("ucpStoreLogo").GetCropUrl(width, height, imageCropMode: ImageCropMode.Max)
            : content.GetStore().LogoImageUrl.GetCropUrl(width, height, imageCropMode: ImageCropMode.Max);
    }

    public static string GetView(this IPublishedContent content) =>
        $"{UmbracoCommercePortalConstants.UmbracoCommercePortalViewPath}/UmbracoCommercePortal{content.Name(Thread.CurrentThread.CurrentCulture.Name).Replace(" ", "")}Page.cshtml";
}
