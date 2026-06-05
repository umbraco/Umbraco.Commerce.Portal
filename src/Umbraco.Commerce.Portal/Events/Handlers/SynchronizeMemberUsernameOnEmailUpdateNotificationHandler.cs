using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Commerce.Portal.Events.Handlers;

public class SynchronizeMemberUsernameOnEmailUpdateNotificationHandler : INotificationAsyncHandler<MemberSavedNotification>
{
    private readonly IMemberService _memberService;

    public SynchronizeMemberUsernameOnEmailUpdateNotificationHandler(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var member in notification.SavedEntities)
        {
            if (!string.IsNullOrWhiteSpace(member.Email) && member.Email != member.Username)
            {
                member.Username = member.Email;
                _memberService.Save(member);
            }
        }

        return Task.CompletedTask;
    }
}
