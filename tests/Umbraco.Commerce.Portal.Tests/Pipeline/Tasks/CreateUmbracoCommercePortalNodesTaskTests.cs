using System.Data;
using System.Linq.Expressions;
using Moq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Commerce.Portal.Pipeline;
using Umbraco.Commerce.Portal.Pipeline.Tasks;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Commerce.Portal.Tests.Pipeline.Tasks;

public class CreateUmbracoCommercePortalNodesTaskTests
{
    private const int SiteRootNodeId = 1000;

    private readonly Mock<IScopeProvider> _scopeProvider = new();
    private readonly Mock<IContentTypeService> _contentTypeService = new();
    private readonly Mock<IContentService> _contentService = new();
    private readonly CreateUmbracoCommercePortalNodesTask _task;
    private int _nextId = 1;

    public CreateUmbracoCommercePortalNodesTaskTests()
    {
        var query = new Mock<IQuery<IContent>>();
        query.Setup(x => x.Where(It.IsAny<Expression<Func<IContent, bool>>>())).Returns(query.Object);
        var sqlContext = new Mock<ISqlContext>();
        sqlContext.Setup(x => x.Query<IContent>()).Returns(query.Object);
        var scope = new Mock<IScope>();
        scope.SetupGet(x => x.SqlContext).Returns(sqlContext.Object);
        _scopeProvider
            .Setup(x => x.CreateScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(scope.Object);

        foreach (string alias in new[]
        {
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalAuthPage,
            UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalManagementPage,
        })
        {
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(x => x.Id).Returns(_nextId++);
            contentType.SetupGet(x => x.Alias).Returns(alias);
            _contentTypeService.Setup(x => x.Get(alias)).Returns(contentType.Object);
        }

        _contentService
            .Setup(x => x.CreateAndSave(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(() => CreateContent());
        _contentService
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(() => CreateContent());

        _task = new CreateUmbracoCommercePortalNodesTask(
            _scopeProvider.Object,
            _contentTypeService.Object,
            _contentService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThePortalIsNotInstalled_RecordsTheNewPortalPageForPublishing()
    {
        SetupExistingPortalPages(0);
        IContent? container = null;
        _contentService
            .Setup(x => x.CreateAndSave("Portal", SiteRootNodeId, UmbracoCommercePortalConstants.ContentTypes.Aliases.PortalContainerPage, It.IsAny<int>()))
            .Returns(() => container = CreateContent());
        InstallPipelineArgs args = CreateArgs();

        await _task.ExecuteAsync(args, CancellationToken.None);

        Assert.NotNull(container);
        Assert.Equal(container.Id, args.Model.CreatedPortalContainerPageId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThePortalIsAlreadyInstalled_DoesNotCreatePagesOrRecordThemForPublishing()
    {
        SetupExistingPortalPages(1);
        InstallPipelineArgs args = CreateArgs();

        await _task.ExecuteAsync(args, CancellationToken.None);

        _contentService.Verify(x => x.CreateAndSave(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        Assert.Null(args.Model.CreatedPortalContainerPageId);
    }

    private static InstallPipelineArgs CreateArgs()
        => new(new InstallPipelineContext { SiteRootNodeId = SiteRootNodeId });

    private IContent CreateContent()
    {
        var content = new Mock<IContent>();
        content.SetupGet(x => x.Id).Returns(_nextId++);
        return content.Object;
    }

    private void SetupExistingPortalPages(long count)
        => _contentService
            .Setup(x => x.GetPagedChildren(SiteRootNodeId, It.IsAny<long>(), It.IsAny<int>(), out count, It.IsAny<string[]?>(), It.IsAny<IQuery<IContent>?>(), It.IsAny<Ordering?>(), It.IsAny<bool>()))
            .Returns([]);
}
