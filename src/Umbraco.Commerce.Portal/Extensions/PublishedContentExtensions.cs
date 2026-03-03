using System;
using System.Collections.Generic;
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
            : "#2563eb";
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

    private static readonly string[] ImagePropertyAliases = [
        "image",
        "images",
        "productImage",
        "productImages",
    ];

    /// <summary>
    /// Retrieves the URL and optional name of the product image from the specified content item.
    /// </summary>
    /// <remarks>If the alias is not specified, the method attempts to locate the product image using a
    /// predefined set of property aliases. This method is useful for content types with varying image property
    /// names.</remarks>
    /// <param name="content">The content item from which to extract the product image information.</param>
    /// <param name="alias">The property alias to use for locating the product image. If null, the method searches common image property
    /// aliases.</param>
    /// <returns>A tuple containing the image URL and optional name if an image is found; otherwise, null.</returns>
    public static (string Url, string? Name)? GetProductImage(this IPublishedContent content, string? alias = null)
    {
        if (alias is not null)
        {
            return GetProductImageUrlByAlias(content, alias);
        }

        foreach (var imageAlias in ImagePropertyAliases)
        {
            if (!content.HasProperty(imageAlias))
            {
                continue;
            }

            return GetProductImageUrlByAlias(content, imageAlias);
        }

        return null;
    }

    private static (string Url, string? Name)? GetProductImageUrlByAlias(IPublishedContent content, string alias)
    {
        var value = content.Value(alias);

        return value switch
        {
            MediaWithCrops mwc => (mwc.GetCropUrl(120, 120)!, mwc.Name),
            IPublishedContent media => (media.GetCropUrl(120, 120)!, media.GetCropUrl(120, 120)!),
            IEnumerable<MediaWithCrops> mwcCol when mwcCol.FirstOrDefault() is { } mwcFirst => (mwcFirst.GetCropUrl(120, 120)!, mwcFirst.Name),
            IEnumerable<IPublishedContent> mediaCol when mediaCol.FirstOrDefault() is { } mediaFirst => (mediaFirst.GetCropUrl(120, 120)!, mediaFirst.Name),
            _ => null,
        };
    }

    public static string GetView(this IPublishedContent content) =>
        $"{UmbracoCommercePortalConstants.UmbracoCommercePortalViewPath}/UmbracoCommercePortal{content.Name(Thread.CurrentThread.CurrentCulture.Name).Replace(" ", "")}Page.cshtml";
}
