using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

/// <summary>
/// The add/edit form of an address in the My Account address book.
/// </summary>
public class AddressBookAddressModel
{
    /// <summary>
    /// Gets or sets the ID of the address being edited, or <c>0</c> for a new address.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a billing (<c>true</c>) or shipping (<c>false</c>) address.
    /// Only used for new addresses; an existing address keeps its kind.
    /// </summary>
    public bool IsBilling { get; set; } = true;

    /// <summary>
    /// Gets or sets the first address line.
    /// </summary>
    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the second address line.
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [Required]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zip or postal code.
    /// </summary>
    [Required]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the country, one of the store's countries.
    /// </summary>
    [Required]
    public Guid? CountryId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the region, one of the country's regions.
    /// </summary>
    public Guid? RegionId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to make this the default address of its kind.
    /// </summary>
    public bool IsDefault { get; set; }
}

