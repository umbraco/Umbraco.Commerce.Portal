using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;
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

    public static IUmbracoBuilder ConfigureUmbracoPortalAuthorization(this IUmbracoBuilder builder)
    {
        builder.Services.ConfigureApplicationCookie(options =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var redirectService = new RedirectService(
                serviceProvider.GetService<IContentTypeService>(),
                serviceProvider.GetService<IContentService>(),
                serviceProvider.GetService<IUmbracoContextFactory>(),
                serviceProvider.GetService<IScopeProvider>());

            options.ReturnUrlParameter = "returnUrl";
            options.Events.OnRedirectToLogin = context =>
            {
                var loginUrl = redirectService.GetLoginNodeUrl();
                if (loginUrl is not null)
                {
                    context.Response.Redirect(loginUrl);
                }

                return System.Threading.Tasks.Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                var loginUrl = redirectService.GetLoginNodeUrl();
                if (loginUrl is not null)
                {
                    context.Response.Redirect(loginUrl);
                }

                return System.Threading.Tasks.Task.CompletedTask;
            };
        });
        return builder;
    }
}
