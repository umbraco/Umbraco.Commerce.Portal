using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Commerce.Portal.Events;

public class OnLogoutNotification : PortalAuthNotificationEventBase<IMember, Guid>
{
    public OnLogoutNotification(IMember member, Guid storeId)
        : base(member, storeId)
    { }
}
