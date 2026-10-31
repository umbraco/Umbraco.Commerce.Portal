using Moq;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Models;
using Umbraco.Commerce.Portal.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;

namespace Umbraco.Commerce.Portal.Tests.Services;

public class PortalCustomerServiceAddressTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid OtherCustomerId = Guid.NewGuid();

    private readonly PortalCustomerServiceFixture _fixture = new();
    private readonly PortalCustomerService _service;

    public PortalCustomerServiceAddressTests() => _service = _fixture.CreateService();

    [Fact]
    public async Task GetCustomerAddressAsync_WhenAddressBelongsToTheCustomer_ReturnsIt()
    {
        CustomerAddress own = _fixture.AddAddress(CustomerId, isBilling: true, isDefault: true);

        Assert.Same(own, await _service.GetCustomerAddressAsync(CustomerId, own.Id));
    }

    [Fact]
    public async Task GetCustomerAddressAsync_WhenAddressBelongsToAnotherCustomer_ReturnsNull()
    {
        CustomerAddress other = _fixture.AddAddress(OtherCustomerId, isBilling: true, isDefault: true);

        Assert.Null(await _service.GetCustomerAddressAsync(CustomerId, other.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetCustomerAddressAsync_WhenAddressDoesNotExist_ReturnsNull(int addressId)
        => Assert.Null(await _service.GetCustomerAddressAsync(CustomerId, addressId));

    [Fact]
    public async Task DeleteAddressAsync_WhenAddressBelongsToAnotherCustomer_RefusesAndDeletesNothing()
    {
        CustomerAddress other = _fixture.AddAddress(OtherCustomerId, isBilling: true, isDefault: true);

        Assert.False(await _service.DeleteAddressAsync(CustomerId, other.Id));
        _fixture.CustomerService.Verify(x => x.DeleteCustomerAddressAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAddressAsync_WhenAddressBelongsToTheCustomer_DeletesIt()
    {
        CustomerAddress own = _fixture.AddAddress(CustomerId, isBilling: false, isDefault: false);

        Assert.True(await _service.DeleteAddressAsync(CustomerId, own.Id));
        _fixture.CustomerService.Verify(x => x.DeleteCustomerAddressAsync(own.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetDefaultAddressAsync_WhenAddressBelongsToAnotherCustomer_RefusesAndSavesNothing()
    {
        CustomerAddress other = _fixture.AddAddress(OtherCustomerId, isBilling: true, isDefault: false);

        Assert.False(await _service.SetDefaultAddressAsync(CustomerId, other.Id));
        Assert.False(other.IsDefault);
        _fixture.CustomerService.Verify(x => x.SaveCustomerAddressAsync(It.IsAny<CustomerAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetDefaultAddressAsync_WhenAddressBelongsToTheCustomer_MakesItDefault()
    {
        CustomerAddress own = _fixture.AddAddress(CustomerId, isBilling: false, isDefault: false);

        Assert.True(await _service.SetDefaultAddressAsync(CustomerId, own.Id));
        _fixture.CustomerService.Verify(x => x.SaveCustomerAddressAsync(It.Is<CustomerAddress>(a => a.Id == own.Id && a.IsDefault), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenEditingAnotherCustomersAddress_ReturnsNotFoundAndSavesNothing()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        _fixture.SetupCountries(denmark);
        CustomerAddress other = _fixture.AddAddress(OtherCustomerId, isBilling: true, isDefault: true);

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(denmark.Id, id: other.Id));

        Assert.Equal(AddressSaveStatus.NotFound, status);
        Assert.Equal("Main Street 1", other.AddressLine1);
        _fixture.CustomerService.Verify(x => x.SaveCustomerAddressAsync(It.IsAny<CustomerAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenEditingOwnAddress_UpdatesItAndKeepsItsKindAndCustomer()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        _fixture.SetupCountries(denmark);
        CustomerAddress own = _fixture.AddAddress(CustomerId, isBilling: false, isDefault: true);

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(denmark.Id, id: own.Id, isBilling: true, isDefault: false));

        Assert.Equal(AddressSaveStatus.Success, status);
        Assert.Equal("Harbour Road 5", own.AddressLine1);
        Assert.Equal(CustomerId, own.CustomerId);
        Assert.False(own.IsBilling);
        Assert.True(own.IsDefault);
        Assert.Equal(denmark.Id, own.CountryId);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenFirstAddressOfItsKind_MakesItDefault()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        _fixture.SetupCountries(denmark);
        _fixture.AddAddress(CustomerId, isBilling: true, isDefault: true);

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(denmark.Id, isBilling: false));

        Assert.Equal(AddressSaveStatus.Success, status);
        CustomerAddress added = _fixture.Addresses.Last();
        Assert.Equal(CustomerId, added.CustomerId);
        Assert.False(added.IsBilling);
        Assert.True(added.IsDefault);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenCountryIsNotInTheStore_ReturnsInvalidCountry()
    {
        _fixture.SetupCountries(await _fixture.CreateCountryAsync("DK", "Denmark"));

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(Guid.NewGuid()));

        Assert.Equal(AddressSaveStatus.InvalidCountry, status);
        Assert.Empty(_fixture.Addresses);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenRegionIsNotInTheCountry_ReturnsInvalidRegion()
    {
        Country us = await _fixture.CreateCountryAsync("US", "United States");
        _fixture.SetupCountries(us);
        _fixture.SetupRegions(us.Id, await _fixture.CreateRegionAsync(us.Id, "US-CA", "California"));

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(us.Id, regionId: Guid.NewGuid()));

        Assert.Equal(AddressSaveStatus.InvalidRegion, status);
        Assert.Empty(_fixture.Addresses);
    }

    [Fact]
    public async Task SaveAddressAsync_WhenRegionIsInTheCountry_SavesIt()
    {
        Country us = await _fixture.CreateCountryAsync("US", "United States");
        Region california = await _fixture.CreateRegionAsync(us.Id, "US-CA", "California");
        _fixture.SetupCountries(us);
        _fixture.SetupRegions(us.Id, california);

        AddressSaveStatus status = await _service.SaveAddressAsync(StoreId, CustomerId, CreateModel(us.Id, regionId: california.Id));

        Assert.Equal(AddressSaveStatus.Success, status);
        Assert.Equal(california.Id, Assert.Single(_fixture.Addresses).RegionId);
    }

    private static AddressBookAddressModel CreateModel(Guid countryId, int id = 0, bool isBilling = true, bool isDefault = false, Guid? regionId = null) => new()
    {
        Id = id,
        IsBilling = isBilling,
        AddressLine1 = "Harbour Road 5",
        City = "Aarhus",
        ZipCode = "8000",
        CountryId = countryId,
        RegionId = regionId,
        IsDefault = isDefault,
    };
}
