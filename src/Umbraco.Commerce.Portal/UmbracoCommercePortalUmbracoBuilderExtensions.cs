using System;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.Portal.Events;
using Umbraco.Commerce.Portal.Events.Handlers;
using Umbraco.Commerce.Portal.Extensions;

namespace Umbraco.Commerce.Portal;

public static class UmbracoCommercePortalUmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoCommercePortal(this IUmbracoBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register pipeline
        builder.AddUmbracoCommercePortalInstallPipeline();

        // Member services
        builder.AddUmbracoCommercePortalMemberServices();

        // Register authentication events
        builder.WithUmbracoCommerceBuilder()
            .WithNotificationEvent<OnLoginNotification>()
            .RegisterHandler<AssignOrderToMemberOnLoginNotificationHandler>();
        builder.AddNotificationHandler<MemberSavedNotification, SynchronizeMemberUsernameOnEmailUpdateNotificationHandler>();

        return builder;
    }
}
