using Umbraco.Commerce.Common.Events;

namespace Umbraco.Commerce.Portal.Events;

public abstract class PortalAuthNotificationEventBase<IMember, Guid> : NotificationEventBase
{
    public IMember Member { get; }

    public Guid StoreId { get; }

    protected PortalAuthNotificationEventBase(IMember member, Guid storeId)
    {
        Member = member;
        StoreId = storeId;
    }
}
