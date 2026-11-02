using System.Collections.Generic;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Portal.Models;

/// <summary>
/// A customer's saved addresses, split by kind. Defaults come first.
/// </summary>
/// <param name="BillingAddresses">The billing addresses.</param>
/// <param name="ShippingAddresses">The shipping addresses.</param>
public sealed record AddressBook(IReadOnlyList<AddressBookEntry> BillingAddresses, IReadOnlyList<AddressBookEntry> ShippingAddresses);

/// <summary>
/// An address in the address book, with its country and region names for display.
/// </summary>
/// <param name="Address">The address.</param>
/// <param name="CountryName">The name of the address's country, or empty when it has none.</param>
/// <param name="RegionName">The name of the address's region, if any.</param>
public sealed record AddressBookEntry(CustomerAddress Address, string CountryName, string? RegionName);
