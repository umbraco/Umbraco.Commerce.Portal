using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

public class AccountModel
{
    [Obsolete("Account actions now act on the signed-in member; this value is ignored. Will be removed in v21.0.0")]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    [Phone]
    public string? Telephone { get; set; }
}

[Obsolete("Billing details now live in the customer's address book (AddressBookAddressModel). Will be removed in v21.0.0")]
public class BillingInformationModel
{
    public Guid Id { get; set; }

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string ZipCode { get; set; } = string.Empty;
}
