using Moq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Portal.Events.Handlers;

namespace Umbraco.Commerce.Portal.Tests.Events.Handlers;

public sealed class SynchronizeMemberUsernameOnEmailUpdateNotificationHandlerTests : IDisposable
{
    private readonly EventMessages _eventMessages = new();
    private readonly Mock<IMemberService> _memberService = new();
    private readonly SynchronizeMemberUsernameOnEmailUpdateNotificationHandler _handler;

    public SynchronizeMemberUsernameOnEmailUpdateNotificationHandlerTests()
    {
        _handler = new SynchronizeMemberUsernameOnEmailUpdateNotificationHandler(_memberService.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailDiffersFromUsername_SetsUsernameAndSaves()
    {
        Mock<IMember> member = CreateMember("new@example.com", "old@example.com");

        await _handler.HandleAsync(CreateNotification(member.Object), CancellationToken.None);

        member.VerifySet(x => x.Username = "new@example.com", Times.Once);
        _memberService.Verify(x => x.Save(member.Object), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailEqualsUsername_DoesNotSave()
    {
        Mock<IMember> member = CreateMember("same@example.com", "same@example.com");

        await _handler.HandleAsync(CreateNotification(member.Object), CancellationToken.None);

        member.VerifySet(x => x.Username = It.IsAny<string>(), Times.Never);
        _memberService.Verify(x => x.Save(It.IsAny<IMember>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WhenEmailIsBlank_DoesNotSave(string email)
    {
        Mock<IMember> member = CreateMember(email, "old@example.com");

        await _handler.HandleAsync(CreateNotification(member.Object), CancellationToken.None);

        member.VerifySet(x => x.Username = It.IsAny<string>(), Times.Never);
        _memberService.Verify(x => x.Save(It.IsAny<IMember>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBatchIsMixed_SavesOnlyChangedMembers()
    {
        Mock<IMember> changed = CreateMember("a@example.com", "x@example.com");
        Mock<IMember> unchanged = CreateMember("b@example.com", "b@example.com");

        await _handler.HandleAsync(CreateNotification(changed.Object, unchanged.Object), CancellationToken.None);

        _memberService.Verify(x => x.Save(changed.Object), Times.Once);
        _memberService.Verify(x => x.Save(unchanged.Object), Times.Never);
    }

    public void Dispose() => _eventMessages.Dispose();

    private static Mock<IMember> CreateMember(string email, string username)
    {
        var member = new Mock<IMember>();
        member.SetupProperty(x => x.Email, email);
        member.SetupProperty(x => x.Username, username);
        return member;
    }

    private MemberSavedNotification CreateNotification(params IMember[] members)
        => new(members, _eventMessages);
}
