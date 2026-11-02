using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Portal.Pipeline;
using Umbraco.Commerce.Portal.Pipeline.Tasks;

namespace Umbraco.Commerce.Portal.Tests.Pipeline.Tasks;

public class CreateUmbracoCommercePortalMemberTypeTaskTests
{
    private readonly Mock<IMemberGroupService> _memberGroupService = new();
    private readonly Mock<IMemberTypeService> _memberTypeService = new();
    private readonly CreateUmbracoCommercePortalMemberTypeTask _task;

    public CreateUmbracoCommercePortalMemberTypeTaskTests()
    {
        _task = new CreateUmbracoCommercePortalMemberTypeTask(
            Mock.Of<IDataTypeService>(),
            _memberGroupService.Object,
            _memberTypeService.Object,
            Mock.Of<IShortStringHelper>(),
            new PropertyEditorCollection(new DataEditorCollection(() => [])),
            Mock.Of<IConfigurationEditorJsonSerializer>(),
            Mock.Of<IUmbracoCommerceApi>());

        _memberGroupService
            .Setup(x => x.CreateAsync(It.IsAny<IMemberGroup>()))
            .ReturnsAsync((IMemberGroup g) => Attempt.SucceedWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.Success, g));
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberTypeExists_DoesNotRecreateIt()
    {
        SetupExistingMemberGroup();
        _memberTypeService
            .Setup(x => x.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType))
            .Returns(Mock.Of<IMemberType>());

        await _task.ExecuteAsync(CreateArgs(), CancellationToken.None);

        _memberTypeService.Verify(x => x.CreateAsync(It.IsAny<IMemberType>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberGroupExists_DoesNotRecreateIt()
    {
        SetupExistingMemberGroup();
        _memberTypeService
            .Setup(x => x.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType))
            .Returns(Mock.Of<IMemberType>());

        await _task.ExecuteAsync(CreateArgs(), CancellationToken.None);

        _memberGroupService.Verify(x => x.CreateAsync(It.IsAny<IMemberGroup>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberGroupMissing_CreatesItWithPortalGroupName()
    {
        _memberGroupService
            .Setup(x => x.GetByNameAsync(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup))
            .ReturnsAsync((IMemberGroup?)null);
        _memberTypeService
            .Setup(x => x.Get(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberType))
            .Returns(Mock.Of<IMemberType>());

        await _task.ExecuteAsync(CreateArgs(), CancellationToken.None);

        _memberGroupService.Verify(
            x => x.CreateAsync(It.Is<IMemberGroup>(g => g.Name == UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup)),
            Times.Once);
    }

    private static InstallPipelineArgs CreateArgs()
        => new(new InstallPipelineContext());

    private void SetupExistingMemberGroup()
        => _memberGroupService
            .Setup(x => x.GetByNameAsync(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup))
            .ReturnsAsync(Mock.Of<IMemberGroup>());
}
