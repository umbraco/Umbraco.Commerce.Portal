using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Portal.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;
using Aliases = Umbraco.Commerce.Portal.UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases;

namespace Umbraco.Commerce.Portal.Tests.Services;

public class LegacyMemberProfileTests
{
    [Fact]
    public void FromMember_WhenMemberHasValues_ReadsThemTrimmed()
    {
        var member = CreateMember("ada@example.com", values: new Dictionary<string, string>
        {
            [Aliases.FirstName] = " Ada ",
            [Aliases.LastName] = "Lovelace",
            [Aliases.Country] = "DK",
            [Aliases.Street] = "Main Street 1",
            [Aliases.City] = "Odense",
            [Aliases.State] = "Syddanmark",
            [Aliases.ZipCode] = "5000",
        });

        var profile = LegacyMemberProfile.FromMember(member.Object);

        Assert.Equal(new LegacyMemberProfile("Ada", "Lovelace", "DK", "Main Street 1", "Odense", "Syddanmark", "5000"), profile);
        Assert.True(profile.HasName);
        Assert.True(profile.HasAddress);
    }

    [Fact]
    public void FromMember_WhenValuesMissing_ReturnsEmptyStrings()
    {
        var profile = LegacyMemberProfile.FromMember(CreateMember("ada@example.com").Object);

        Assert.Equal(new LegacyMemberProfile(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty), profile);
        Assert.False(profile.HasName);
        Assert.False(profile.HasAddress);
    }

    [Fact]
    public void FromMember_WhenOnlyAStateIsSet_HasNoAddress()
    {
        var member = CreateMember("ada@example.com", values: new Dictionary<string, string> { [Aliases.State] = "Syddanmark" });

        Assert.False(LegacyMemberProfile.FromMember(member.Object).HasAddress);
    }

    [Fact]
    public void FromMember_WhenMemberIsNull_Throws()
    {
        IMember member = null!;

        Assert.Throws<ArgumentNullException>(() => LegacyMemberProfile.FromMember(member));
    }
}
