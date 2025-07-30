using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Commerce.Portal.Events;

public class OnLoginNotification : INotification
{
    public IMember LoggedInMember { get; }

    public Guid StoreId { get; }

    public OnLoginNotification(IMember loggedInMember, Guid storeId)
    {
        LoggedInMember = loggedInMember;
        StoreId = storeId;
    }
}
