using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.Portal.Pipeline;
using Umbraco.Commerce.Portal.Pipeline.Tasks;
using Umbraco.Commerce.Portal.Services;

namespace Umbraco.Commerce.Portal.Extensions;

internal static class CompositionExtensions
{
    public static IUmbracoBuilder AddUmbracoCommercePortalInstallPipeline(this IUmbracoBuilder builder)
    {
        IUmbracoCommerceBuilder commerceBuilder = builder.WithUmbracoCommerceBuilder();
        commerceBuilder.WithPipeline<InstallAsyncPipelineTask, InstallPipelineContext>()
            .Add<ConfigureUmbracoCommercePortalStoreTask>()
            .Add<CreateUmbracoCommercePortalMemberTypeTask>()
            .Add<CreateUmbracoCommercePortalDocumentTypesTask>()
            .Add<CreateUmbracoCommercePortalNodesTask>();

        return builder;
    }

    public static IUmbracoBuilder AddUmbracoCommercePortalMemberServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<PortalMemberService>();
        return builder;
    }
}
