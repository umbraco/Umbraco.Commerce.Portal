using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Commerce.Portal.Extensions;

namespace Umbraco.Commerce.Portal;

public static class UmbracoCommercePortalUmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoCommercePortal(this IUmbracoBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/portal/login";
            options.AccessDeniedPath = "/portal/login";
            options.ReturnUrlParameter = "returnUrl";
        });

        // Register pipeline
        builder.AddUmbracoCommercePortalInstallPipeline();

        // Member services
        builder.AddUmbracoCommercePortalMemberServices();

        return builder;
    }
}
