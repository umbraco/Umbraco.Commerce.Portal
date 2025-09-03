using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Commerce.Portal.Events;

public class OnLoginNotification : PortalAuthNotificationEventBase<IMember, Guid>
{
    public OnLoginNotification(IMember member, Guid storeId)
        : base(member, storeId)
    { }
}
