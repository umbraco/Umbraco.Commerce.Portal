using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Portal.Extensions;

namespace Umbraco.Commerce.Portal.Tests.Extensions;

public class PortalMemberExtensionsTests
{
    [Fact]
    public void ToAccountModel_WhenMemberHasValues_MapsThem()
    {
        var key = Guid.NewGuid();
        Mock<IMember> member = CreateMember(key, new Dictionary<string, string>
        {
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName] = "Ada",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName] = "Lovelace",
        });
        member.SetupGet(x => x.Email).Returns("ada@example.com");

        var model = member.Object.ToAccountModel();

        Assert.Equal(key, model.Id);
        Assert.Equal("ada@example.com", model.Email);
        Assert.Equal("Ada", model.FirstName);
        Assert.Equal("Lovelace", model.LastName);
    }

    [Fact]
    public void ToAccountModel_WhenValuesMissing_ReturnsEmptyStrings()
    {
        Mock<IMember> member = CreateMember(Guid.NewGuid(), []);
        member.SetupGet(x => x.Email).Returns("ada@example.com");

        var model = member.Object.ToAccountModel();

        Assert.Equal(string.Empty, model.FirstName);
        Assert.Equal(string.Empty, model.LastName);
    }

    [Fact]
    public void ToAccountModel_WhenMemberIsNull_Throws()
    {
        IMember member = null!;

        Assert.Throws<ArgumentNullException>(() => member.ToAccountModel());
    }

    [Fact]
    public void ToBillingInformationModel_WhenMemberHasValues_MapsThem()
    {
        var key = Guid.NewGuid();
        Mock<IMember> member = CreateMember(key, new Dictionary<string, string>
        {
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Country] = "DK",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.Street] = "Main Street 1",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.City] = "Odense",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.State] = "Syddanmark",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.ZipCode] = "5000",
        });

        var model = member.Object.ToBillingInformationModel();

        Assert.Equal(key, model.Id);
        Assert.Equal("DK", model.Country);
        Assert.Equal("Main Street 1", model.Street);
        Assert.Equal("Odense", model.City);
        Assert.Equal("Syddanmark", model.State);
        Assert.Equal("5000", model.ZipCode);
    }

    [Fact]
    public void ToBillingInformationModel_WhenValuesMissing_ReturnsEmptyStrings()
    {
        Mock<IMember> member = CreateMember(Guid.NewGuid(), []);

        var model = member.Object.ToBillingInformationModel();

        Assert.Equal(string.Empty, model.Country);
        Assert.Equal(string.Empty, model.Street);
        Assert.Equal(string.Empty, model.City);
        Assert.Equal(string.Empty, model.State);
        Assert.Equal(string.Empty, model.ZipCode);
    }

    [Fact]
    public void ToBillingInformationModel_WhenMemberIsNull_Throws()
    {
        IMember member = null!;

        Assert.Throws<ArgumentNullException>(() => member.ToBillingInformationModel());
    }

    private static Mock<IMember> CreateMember(Guid key, Dictionary<string, string> values)
    {
        var member = new Mock<IMember>();
        member.SetupGet(x => x.Key).Returns(key);
        member
            .Setup(x => x.GetValue<string>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns((string alias, string? _, string? _, bool _) => values.GetValueOrDefault(alias));
        return member;
    }
}
