using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Commerce.Portal.Events;

public class OnLogoutNotification : INotification
{
    public IMember LoggedOutMember { get; }

    public OnLogoutNotification(IMember loggedOutMember) => LoggedOutMember = loggedOutMember;
}
