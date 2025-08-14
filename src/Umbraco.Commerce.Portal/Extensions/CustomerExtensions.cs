using System;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Portal.Models;

namespace Umbraco.Commerce.Portal.Extensions;

public static class CustomerExtensions
{
    public static CustomerModel GetCustomerModel(this OrderReadOnly order) =>
        new CustomerModel
        {
            FirstName = order.CustomerInfo.FirstName,
            LastName = order.CustomerInfo.LastName,
            Email = order.CustomerInfo.Email,
            Telephone = order.Properties["billingTelephone"],
        };

    public static AddressModel GetBillingAddressModel(this OrderReadOnly order)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return new AddressModel
        {
            AddressLine1 = order.Properties["billingAddressLine1"],
            AddressLine2 = order.Properties["billingAddressLine2"],
            City = order.Properties["billingCity"],
            ZipCode = order.Properties["billingZipCode"],
        };
    }

    public static AddressModel GetShippingAddressModel(this OrderReadOnly order)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return new AddressModel
        {
            AddressLine1 = order.Properties["shippingAddressLine1"],
            AddressLine2 = order.Properties["shippingAddressLine2"],
            City = order.Properties["shippingCity"],
            ZipCode = order.Properties["shippingZipCode"],
        };
    }

}
