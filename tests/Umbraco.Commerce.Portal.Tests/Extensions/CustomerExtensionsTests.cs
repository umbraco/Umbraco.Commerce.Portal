using Moq;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Extensions;
using Umbraco.Commerce.Portal.Tests.Services;
using static Umbraco.Commerce.Portal.Tests.Services.PortalCustomerServiceFixture;

namespace Umbraco.Commerce.Portal.Tests.Extensions;

public class CustomerExtensionsTests
{
    private readonly PortalCustomerServiceFixture _fixture = new();

    [Fact]
    public async Task GetCountryNameAsync_WhenShippingCountryDiffersFromBilling_ReturnsTheShippingCountry()
    {
        Country denmark = await _fixture.CreateCountryAsync("DK", "Denmark");
        Country sweden = await _fixture.CreateCountryAsync("SE", "Sweden");
        _fixture.SetupCountries(denmark, sweden);
        var billing = new Address("Main Street 1", "Odense", "DK", "5000");
        var shipping = new Address("Storgatan 1", "Malmö", "SE", "21120");

        Assert.Equal("Denmark", await _fixture.CommerceApi.Object.GetCountryNameAsync(StoreId, billing));
        Assert.Equal("Sweden", await _fixture.CommerceApi.Object.GetCountryNameAsync(StoreId, shipping));
    }

    [Fact]
    public async Task GetCountryNameAsync_WhenAddressIsNull_ReturnsNullWithoutALookup()
    {
        Assert.Null(await _fixture.CommerceApi.Object.GetCountryNameAsync(StoreId, null));

        _fixture.CommerceApi.Verify(x => x.GetCountryAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetCountryNameAsync_WhenAddressHasNoCountry_ReturnsNullWithoutALookup(string? countryIsoCode)
    {
        var address = new Address("Main Street 1", "Odense", countryIsoCode!, "5000");

        Assert.Null(await _fixture.CommerceApi.Object.GetCountryNameAsync(StoreId, address));

        _fixture.CommerceApi.Verify(x => x.GetCountryAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCountryNameAsync_WhenCountryIsNotInTheStore_ReturnsNull()
    {
        var address = new Address("Main Street 1", "Odense", "XX", "5000");

        Assert.Null(await _fixture.CommerceApi.Object.GetCountryNameAsync(StoreId, address));
    }
}
