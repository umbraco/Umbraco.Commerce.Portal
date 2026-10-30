using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Portal.Web.Controllers;

namespace Umbraco.Commerce.Portal.Tests.Web.Controllers;

public sealed class UmbracoCommercePortalApiControllerTests : IDisposable
{
    private readonly Mock<IUmbracoCommerceApi> _commerceApi = new();
    private readonly Mock<IContentService> _contentService = new();
    private readonly UmbracoCommercePortalApiController _controller;

    public UmbracoCommercePortalApiControllerTests()
    {
        _controller = new UmbracoCommercePortalApiController(_commerceApi.Object, _contentService.Object);
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void InstallUmbracoCommercePortal_WhenInspected_IsHttpPostOnInstallRoute()
    {
        MethodInfo action = typeof(UmbracoCommercePortalApiController).GetMethod(nameof(UmbracoCommercePortalApiController.InstallUmbracoCommercePortal))!;

        var attribute = Assert.Single(action.GetCustomAttributes<HttpPostAttribute>());

        Assert.Equal("install", attribute.Template);
        Assert.Empty(action.GetCustomAttributes<HttpGetAttribute>());
    }

    [Fact]
    public void Controller_WhenInspected_RequiresSettingsSectionAccess()
    {
        var policies = typeof(UmbracoCommercePortalApiController)
            .GetCustomAttributes<AuthorizeAttribute>()
            .Select(x => x.Policy);

        Assert.Contains(AuthorizationPolicies.SectionAccessSettings, policies);
    }

    [Fact]
    public async Task InstallUmbracoCommercePortal_WhenSiteRootIdIsNull_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.InstallUmbracoCommercePortal(null));
    }

    [Fact]
    public async Task InstallUmbracoCommercePortal_WhenSiteRootNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _contentService.Setup(x => x.GetById(id)).Returns((IContent?)null);

        object result = await _controller.InstallUmbracoCommercePortal(id);

        AssertFailure(result, "site root node");
    }

    [Fact]
    public async Task InstallUmbracoCommercePortal_WhenNoStorePickerOnNodeOrAncestors_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        Mock<IContent> root = CreateContent(parentId: 10, hasStorePicker: false);
        Mock<IContent> parent = CreateContent(parentId: -1, hasStorePicker: false);
        _contentService.Setup(x => x.GetById(id)).Returns(root.Object);
        _contentService.Setup(x => x.GetById(10)).Returns(parent.Object);

        object result = await _controller.InstallUmbracoCommercePortal(id);

        AssertFailure(result, "store");
        _commerceApi.Verify(x => x.GetStoreAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task InstallUmbracoCommercePortal_WhenStorePickerHasNoValue_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        Mock<IContent> root = CreateContent(parentId: -1, hasStorePicker: true);
        root.Setup(x => x.GetValue(Cms.Constants.Properties.StorePropertyAlias, null, null, false)).Returns(default(Guid?));
        _contentService.Setup(x => x.GetById(id)).Returns(root.Object);

        object result = await _controller.InstallUmbracoCommercePortal(id);

        AssertFailure(result, "store");
    }

    private static Mock<IContent> CreateContent(int parentId, bool hasStorePicker)
    {
        var content = new Mock<IContent>();
        content.SetupGet(x => x.ParentId).Returns(parentId);
        content.Setup(x => x.HasProperty(Cms.Constants.Properties.StorePropertyAlias)).Returns(hasStorePicker);
        return content;
    }

    private static void AssertFailure(object result, string messageFragment)
    {
        Type type = result.GetType();
        Assert.False((bool)type.GetProperty("success")!.GetValue(result)!);
        var message = (string)type.GetProperty("message")!.GetValue(result)!;
        Assert.Contains(messageFragment, message, StringComparison.Ordinal);
    }
}
