using Moq;
using Umbraco.Commerce.Common.Models;
using Umbraco.Commerce.Common.Validation;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;

namespace Umbraco.Commerce.Portal.Tests.Services;

public class PortalCustomerServiceResolveTests
{
    private readonly PortalCustomerServiceFixture _fixture = new();
    private readonly PortalCustomerService _service;

    public PortalCustomerServiceResolveTests() => _service = _fixture.CreateService();

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenLinkedByMemberKey_ReturnsItWithoutSaving()
    {
        var member = CreateMember("ada@example.com");
        Customer linked = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace", member.Object.Key, imported: true);
        _fixture.CustomerService.Setup(x => x.GetCustomerByMemberKeyAsync(StoreId, member.Object.Key)).ReturnsAsync(linked);

        CustomerReadOnly result = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Equal(linked.Id, result.Id);
        Assert.Empty(_fixture.SavedCustomers);
        _fixture.CustomerService.Verify(x => x.GetCustomersByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenOnlyFoundByEmail_LinksItToTheMember()
    {
        var member = CreateMember("ada@example.com");
        Customer otherStore = await _fixture.CreateCustomerAsync("ada@example.com", storeId: OtherStoreId);
        Customer byEmail = await _fixture.CreateCustomerAsync("ADA@example.com", "Ada", "Lovelace");
        _fixture.CustomerService.Setup(x => x.GetCustomersByEmailAsync("ada@example.com")).ReturnsAsync([otherStore, byEmail]);

        CustomerReadOnly result = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Equal(byEmail.Id, result.Id);
        Customer saved = Assert.Single(_fixture.SavedCustomers);
        Assert.Same(byEmail, saved);
        Assert.Equal(member.Object.Key, saved.MemberKey);
        Assert.Null(otherStore.MemberKey);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenNoneExists_CreatesOneFromTheMember()
    {
        var member = CreateMember("ada@example.com", "Someone Else", new Dictionary<string, string>
        {
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.FirstName] = "Ada",
            [UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases.LastName] = "Lovelace",
        });

        CustomerReadOnly result = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Customer saved = Assert.Single(_fixture.SavedCustomers);
        Assert.Equal(saved.Id, result.Id);
        Assert.Equal(StoreId, saved.StoreId);
        Assert.Equal("ada@example.com", saved.Email);
        Assert.Equal("Ada", saved.FirstName);
        Assert.Equal("Lovelace", saved.LastName);
        Assert.Equal(member.Object.Key, saved.MemberKey);
        Assert.True(saved.Properties.ContainsKey(UmbracoCommercePortalConstants.Customers.LegacyProfileImportedPropertyAlias));
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenMemberHasNoNameProperties_SplitsTheMemberName()
    {
        var member = CreateMember("ada@example.com", "Ada King Lovelace");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Customer saved = Assert.Single(_fixture.SavedCustomers);
        Assert.Equal("Ada", saved.FirstName);
        Assert.Equal("King Lovelace", saved.LastName);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenNamesAreGiven_UsesThemForANewCustomer()
    {
        var member = CreateMember("ada@example.com", "Ada Lovelace");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object, "Augusta", "Byron");

        Customer saved = Assert.Single(_fixture.SavedCustomers);
        Assert.Equal("Augusta", saved.FirstName);
        Assert.Equal("Byron", saved.LastName);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenExistingCustomerHasAName_KeepsIt()
    {
        var member = CreateMember("ada@example.com", "Someone Else");
        Customer byEmail = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace");
        _fixture.CustomerService.Setup(x => x.GetCustomersByEmailAsync("ada@example.com")).ReturnsAsync([byEmail]);

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object, "Augusta", "Byron");

        Assert.Equal("Ada", byEmail.FirstName);
        Assert.Equal("Lovelace", byEmail.LastName);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenLinkedCustomerHasAnOldEmail_UpdatesItsEmail()
    {
        var member = CreateMember("new@example.com");
        Customer linked = await _fixture.CreateCustomerAsync("old@example.com", "Ada", "Lovelace", member.Object.Key, imported: true);
        _fixture.CustomerService.Setup(x => x.GetCustomerByMemberKeyAsync(StoreId, member.Object.Key)).ReturnsAsync(linked);

        CustomerReadOnly result = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Equal(linked.Id, result.Id);
        Assert.Equal("new@example.com", Assert.Single(_fixture.SavedCustomers).Email);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenAnotherRequestCreatesItFirst_ReturnsThatCustomer()
    {
        var member = CreateMember("ada@example.com");
        Customer createdElsewhere = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace", member.Object.Key, imported: true);
        _fixture.CustomerService
            .SetupSequence(x => x.GetCustomerByMemberKeyAsync(StoreId, member.Object.Key))
            .ReturnsAsync((CustomerReadOnly?)null)
            .ReturnsAsync(createdElsewhere);
        _fixture.CustomerService
            .Setup(x => x.SaveCustomerAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationError("A customer with the email 'ada@example.com' already exists")]));

        CustomerReadOnly result = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Equal(createdElsewhere.Id, result.Id);
    }
}
