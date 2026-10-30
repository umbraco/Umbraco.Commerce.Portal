using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Portal.Pipeline;
using Umbraco.Commerce.Portal.Pipeline.Tasks;

namespace Umbraco.Commerce.Portal.Tests.Pipeline.Tasks;

public class ConfigureUmbracoCommercePortalPublicAccessTaskTests
{
    private const int SiteRootNodeId = 1000;

    private readonly Mock<IPublicAccessService> _publicAccessService = new();
    private readonly Mock<IContentTypeService> _contentTypeService = new();
    private readonly Mock<IContentService> _contentService = new();
    private readonly ConfigureUmbracoCommercePortalPublicAccessTask _task;

    public ConfigureUmbracoCommercePortalPublicAccessTaskTests()
    {
        _task = new ConfigureUmbracoCommercePortalPublicAccessTask(
            _publicAccessService.Object,
            _contentTypeService.Object,
            _contentService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoEntryExists_CreatesEntryWithMemberGroupRule()
    {
        IContent container = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, 1, SiteRootNodeId);
        IContent management = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage, 2, container.Id);
        IContent login = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage, 3, container.Id, UmbracoCommercePortalConstants.ContentNodes.PortalAuthLoginPageNodeName);
        PublicAccessEntry? saved = null;
        _publicAccessService.Setup(x => x.GetEntryForContent(management)).Returns((PublicAccessEntry?)null);
        _publicAccessService
            .Setup(x => x.Save(It.IsAny<PublicAccessEntry>()))
            .Callback((PublicAccessEntry e) => saved = e);

        await _task.ExecuteAsync(CreateArgs(), CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(management.Id, saved.ProtectedNodeId);
        Assert.Equal(login.Id, saved.LoginNodeId);
        Assert.Equal(login.Id, saved.NoAccessNodeId);
        PublicAccessRule rule = Assert.Single(saved.Rules);
        Assert.Equal(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalMemberGroup, rule.RuleValue);
        Assert.Equal("MemberRole", rule.RuleType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEntryExists_SavesWithoutAddingRule()
    {
        IContent container = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, 1, SiteRootNodeId);
        IContent management = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage, 2, container.Id);
        IContent login = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage, 3, container.Id, UmbracoCommercePortalConstants.ContentNodes.PortalAuthLoginPageNodeName);
        var existing = new PublicAccessEntry(management, login, login, []);
        _publicAccessService.Setup(x => x.GetEntryForContent(management)).Returns(existing);

        await _task.ExecuteAsync(CreateArgs(), CancellationToken.None);

        _publicAccessService.Verify(x => x.Save(existing), Times.Once);
        Assert.Empty(existing.Rules);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContainerNodeMissing_Throws()
    {
        SetupContentType(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, 1);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _task.ExecuteAsync(CreateArgs(), CancellationToken.None));

        Assert.Contains("Container", ex.Message, StringComparison.Ordinal);
        _publicAccessService.Verify(x => x.Save(It.IsAny<PublicAccessEntry>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManagementNodeMissing_Throws()
    {
        SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, 1, SiteRootNodeId);
        SetupContentType(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage, 2);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _task.ExecuteAsync(CreateArgs(), CancellationToken.None));

        Assert.Contains("Management", ex.Message, StringComparison.Ordinal);
        _publicAccessService.Verify(x => x.Save(It.IsAny<PublicAccessEntry>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginNodeMissing_Throws()
    {
        IContent container = SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, 1, SiteRootNodeId);
        SetupNode(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage, 2, container.Id);
        SetupContentType(UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage, 3);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _task.ExecuteAsync(CreateArgs(), CancellationToken.None));

        Assert.Contains("Login", ex.Message, StringComparison.Ordinal);
        _publicAccessService.Verify(x => x.Save(It.IsAny<PublicAccessEntry>()), Times.Never);
    }

    private static InstallPipelineArgs CreateArgs()
        => new(new InstallPipelineContext { SiteRootNodeId = SiteRootNodeId });

    private static IContent CreateContent(int id, int parentId, string name)
    {
        var content = new Mock<IContent>();
        content.SetupGet(x => x.Id).Returns(id);
        content.SetupGet(x => x.Key).Returns(Guid.NewGuid());
        content.SetupGet(x => x.ParentId).Returns(parentId);
        content.SetupGet(x => x.Name).Returns(name);
        return content.Object;
    }

    // Registers a content type (id == node id) with a single node of that type, and returns the node.
    private IContent SetupNode(string contentTypeAlias, int id, int parentId, string name = "Node")
    {
        IContent node = CreateContent(id, parentId, name);
        SetupContentType(contentTypeAlias, id, [node]);
        return node;
    }

    private void SetupContentType(string alias, int id, IContent[]? nodes = null)
    {
        var contentType = new Mock<IContentType>();
        contentType.SetupGet(x => x.Id).Returns(id);
        _contentTypeService.Setup(x => x.Get(alias)).Returns(contentType.Object);

        long total = 0;
        _contentService
            .Setup(x => x.GetPagedOfType(id, 0, 10, out total, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering?>()))
            .Returns(nodes ?? []);
    }
}
