using System;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Extensions;

public static class CustomerExtensions
{
    public static CustomerModel GetCustomerModel(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig) =>
        new CustomerModel
        {
            FirstName = order.CustomerInfo.FirstName,
            LastName = order.CustomerInfo.LastName,
            Email = order.CustomerInfo.Email,
            Telephone = order.Properties[orderPropertyConfig.Customer.Telephone.Alias],
        };

    public static AddressModel GetBillingAddressModel(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return new AddressModel
        {
            AddressLine1 = order.Properties[orderPropertyConfig.Billing.AddressLine1.Alias],
            AddressLine2 = order.Properties[orderPropertyConfig.Billing.AddressLine2.Alias],
            City = order.Properties[orderPropertyConfig.Billing.City.Alias],
            ZipCode = order.Properties[orderPropertyConfig.Billing.ZipCode.Alias],
        };
    }

    public static AddressModel GetShippingAddressModel(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return new AddressModel
        {
            AddressLine1 = order.Properties[orderPropertyConfig.Shipping.AddressLine1.Alias],
            AddressLine2 = order.Properties[orderPropertyConfig.Shipping.AddressLine2.Alias],
            City = order.Properties[orderPropertyConfig.Shipping.City.Alias],
            ZipCode = order.Properties[orderPropertyConfig.Shipping.ZipCode.Alias],
        };
    }

    /// <summary>
    /// Gets the name of the store country for the address's country code, or <c>null</c> when the
    /// address is missing, has no country, or the country isn't in the store.
    /// </summary>
    /// <param name="commerceApi">The Commerce API.</param>
    /// <param name="storeId">The store the country belongs to.</param>
    /// <param name="address">The address, such as an order's billing or shipping address.</param>
    /// <returns>The country name, or <c>null</c>.</returns>
    internal static async Task<string?> GetCountryNameAsync(this IUmbracoCommerceApi commerceApi, Guid storeId, Address? address)
    {
        if (string.IsNullOrWhiteSpace(address?.CountryIsoCode))
        {
            return null;
        }

        CountryReadOnly? country = await commerceApi.GetCountryAsync(storeId, address.CountryIsoCode);
        return country?.Name;
    }
}
