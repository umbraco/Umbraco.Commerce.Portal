using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Commerce.Portal.Events.Handlers;

public class SynchronizeMemberUsernameOnEmailUpdateNotificationHandler : INotificationHandler<MemberSavedNotification>
{
    private readonly IMemberService _memberService;

    public SynchronizeMemberUsernameOnEmailUpdateNotificationHandler(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public void Handle(MemberSavedNotification notification)
    {
        foreach (var member in notification.SavedEntities)
        {
            if (!string.IsNullOrWhiteSpace(member.Email) && member.Email != member.Username)
            {
                member.Username = member.Email;
                _memberService.Save(member);
            }
        }
    }
}
