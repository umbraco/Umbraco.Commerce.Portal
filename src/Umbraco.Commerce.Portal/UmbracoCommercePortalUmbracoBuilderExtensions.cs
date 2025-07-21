using System;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Commerce.Portal.Extensions;

namespace Umbraco.Commerce.Portal;

public static class UmbracoCommercePortalUmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoCommercePortal(this IUmbracoBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register pipeline
        builder.AddUmbracoCommercePortalInstallPipeline();

        return builder;
    }
}
