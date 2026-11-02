using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Portal.Pipeline;
using Umbraco.Commerce.Portal.Pipeline.Tasks;

namespace Umbraco.Commerce.Portal.Tests.Pipeline.Tasks;

public class PublishUmbracoCommercePortalNodesTaskTests
{
    private const int ContainerId = 42;

    private readonly Mock<IContentService> _contentService = new();
    private readonly IContent _container = Mock.Of<IContent>(x => x.Id == ContainerId);
    private readonly PublishUmbracoCommercePortalNodesTask _task;

    public PublishUmbracoCommercePortalNodesTaskTests()
    {
        _contentService.Setup(x => x.GetById(ContainerId)).Returns(_container);
        _task = new PublishUmbracoCommercePortalNodesTask(
            _contentService.Object,
            NullLogger<PublishUmbracoCommercePortalNodesTask>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThisInstallCreatedThePages_PublishesThePortalPageWithItsDescendants()
    {
        SetupPublishBranch(PublishResultType.SuccessPublish);

        await _task.ExecuteAsync(CreateArgs(ContainerId), CancellationToken.None);

        _contentService.Verify(
            x => x.PublishBranch(_container, PublishBranchFilter.IncludeUnpublished, It.Is<string[]>(c => c.SequenceEqual(new[] { "*" })), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThePagesAlreadyExisted_DoesNotPublish()
    {
        await _task.ExecuteAsync(CreateArgs(null), CancellationToken.None);

        _contentService.Verify(
            x => x.PublishBranch(It.IsAny<IContent>(), It.IsAny<PublishBranchFilter>(), It.IsAny<string[]>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublishingFails_ReturnsSuccess()
    {
        SetupPublishBranch(PublishResultType.FailedPublishPathNotPublished);

        var result = await _task.ExecuteAsync(CreateArgs(ContainerId), CancellationToken.None);

        Assert.True(result.Success);
    }

    private static InstallPipelineArgs CreateArgs(int? createdContainerId)
        => new(new InstallPipelineContext { CreatedPortalContainerPageId = createdContainerId });

    private void SetupPublishBranch(PublishResultType resultType)
        => _contentService
            .Setup(x => x.PublishBranch(It.IsAny<IContent>(), It.IsAny<PublishBranchFilter>(), It.IsAny<string[]>(), It.IsAny<int>()))
            .Returns((IContent content, PublishBranchFilter _, string[] _, int _) => [new PublishResult(resultType, new EventMessages(), content)]);
}
