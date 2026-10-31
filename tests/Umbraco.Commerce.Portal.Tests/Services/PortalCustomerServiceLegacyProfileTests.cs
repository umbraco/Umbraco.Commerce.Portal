using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;
using Aliases = Umbraco.Commerce.Portal.UmbracoCommercePortalConstants.ContentTypes.MemberTypeAliases;

namespace Umbraco.Commerce.Portal.Tests.Services;

/// <summary>
/// The first time a member's customer is set up, the member's old Portal profile properties are copied onto it.
/// </summary>
public class PortalCustomerServiceLegacyProfileTests
{
    private readonly PortalCustomerServiceFixture _fixture = new();
    private readonly PortalCustomerService _service;

    public PortalCustomerServiceLegacyProfileTests() => _service = _fixture.CreateService();

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenMemberHasAnOldAddress_AddsItAsTheDefaultBillingAddress()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        Region southDenmark = await _fixture.CreateRegionAsync(denmark.Id, "DK-83", "Syddanmark");
        _fixture.SetupCountries(denmark);
        _fixture.SetupRegions(denmark.Id, southDenmark);
        Mock<IMember> member = CreateMemberWithAddress(denmark.Id.ToString(), "syddanmark");

        CustomerReadOnly customer = await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        CustomerAddress address = Assert.Single(_fixture.Addresses);
        Assert.Equal(customer.Id, address.CustomerId);
        Assert.True(address.IsBilling);
        Assert.True(address.IsDefault);
        Assert.Equal("Main Street 1", address.AddressLine1);
        Assert.Equal("Odense", address.City);
        Assert.Equal("5000", address.ZipCode);
        Assert.Equal(denmark.Id, address.CountryId);
        Assert.Equal(southDenmark.Id, address.RegionId);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenOldCountryIsACodeAndStateIsARegionCode_MatchesBoth()
    {
        Country us = await _fixture.CreateCountryAsync("US", "United States");
        Region california = await _fixture.CreateRegionAsync(us.Id, "US-CA", "California");
        _fixture.SetupCountries(us);
        _fixture.SetupRegions(us.Id, california);
        Mock<IMember> member = CreateMemberWithAddress("US", "ca");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        CustomerAddress address = Assert.Single(_fixture.Addresses);
        Assert.Equal(us.Id, address.CountryId);
        Assert.Equal(california.Id, address.RegionId);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenStateMatchesNoRegion_LeavesTheRegionOut()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        _fixture.SetupCountries(denmark);
        _fixture.SetupRegions(denmark.Id, await _fixture.CreateRegionAsync(denmark.Id, "DK-84", "Hovedstaden"));
        Mock<IMember> member = CreateMemberWithAddress(denmark.Id.ToString(), "Atlantis");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        CustomerAddress address = Assert.Single(_fixture.Addresses);
        Assert.Equal(denmark.Id, address.CountryId);
        Assert.Null(address.RegionId);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenOldCountryBelongsToAnotherStore_LeavesTheCountryOut()
    {
        Country elsewhere = await _fixture.CreateCountryAsync("DK", "Denmark", OtherStoreId);
        _fixture.CommerceApi.Setup(x => x.GetCountryAsync(elsewhere.Id)).ReturnsAsync(elsewhere);
        Mock<IMember> member = CreateMemberWithAddress(elsewhere.Id.ToString(), "Syddanmark");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        CustomerAddress address = Assert.Single(_fixture.Addresses);
        Assert.Null(address.CountryId);
        Assert.Null(address.RegionId);
        Assert.Equal("Main Street 1", address.AddressLine1);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenAlreadyImported_DoesNotAddTheAddressAgain()
    {
        Mock<IMember> member = CreateMemberWithAddress("DK", string.Empty);
        Customer linked = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace", member.Object.Key, imported: true);
        _fixture.CustomerService.Setup(x => x.GetCustomerByMemberKeyAsync(StoreId, member.Object.Key)).ReturnsAsync(linked);

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Empty(_fixture.Addresses);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenCustomerAlreadyHasTheAddress_DoesNotDuplicateIt()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        _fixture.SetupCountries(denmark);
        Mock<IMember> member = CreateMemberWithAddress("DK", string.Empty);
        Customer byEmail = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace");
        _fixture.CustomerService.Setup(x => x.GetCustomersByEmailAsync("ada@example.com")).ReturnsAsync([byEmail]);
        _fixture.AddAddress(byEmail.Id, isBilling: true, isDefault: true, line1: "MAIN STREET 1", countryId: denmark.Id);

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Single(_fixture.Addresses);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenCustomerHasADefaultBillingAddress_AddsTheOldAddressAsNonDefault()
    {
        Mock<IMember> member = CreateMemberWithAddress(string.Empty, string.Empty);
        Customer byEmail = await _fixture.CreateCustomerAsync("ada@example.com", "Ada", "Lovelace");
        _fixture.CustomerService.Setup(x => x.GetCustomersByEmailAsync("ada@example.com")).ReturnsAsync([byEmail]);
        CustomerAddress existing = _fixture.AddAddress(byEmail.Id, isBilling: true, isDefault: true, line1: "Other Street 2");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Equal(2, _fixture.Addresses.Count);
        CustomerAddress imported = _fixture.Addresses.Single(x => x.Id != existing.Id);
        Assert.False(imported.IsDefault);
        Assert.True(existing.IsDefault);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenMemberHasNoOldAddress_AddsNoAddress()
    {
        Mock<IMember> member = CreateMember("ada@example.com", "Ada Lovelace");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        Assert.Empty(_fixture.Addresses);
        _fixture.CustomerService.Verify(x => x.SaveCustomerAddressAsync(It.IsAny<CustomerAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateCustomerAsync_WhenImporting_NeverWritesTheMember()
    {
        Mock<IMember> member = CreateMemberWithAddress("DK", "Syddanmark");

        await _service.GetOrCreateCustomerAsync(StoreId, member.Object);

        member.Verify(x => x.SetValue(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        _fixture.MemberService.Verify(x => x.Save(It.IsAny<IMember>(), It.IsAny<int>()), Times.Never);
    }

    private static Mock<IMember> CreateMemberWithAddress(string country, string state) => CreateMember("ada@example.com", "Ada Lovelace", new Dictionary<string, string>
    {
        [Aliases.FirstName] = "Ada",
        [Aliases.LastName] = "Lovelace",
        [Aliases.Country] = country,
        [Aliases.Street] = "Main Street 1",
        [Aliases.City] = "Odense",
        [Aliases.State] = state,
        [Aliases.ZipCode] = "5000",
    });
}
