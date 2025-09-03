using System;
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

}
