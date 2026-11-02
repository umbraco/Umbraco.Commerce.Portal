namespace Umbraco.Commerce.Portal.Models;

/// <summary>
/// The outcome of saving an address book address.
/// </summary>
public enum AddressSaveStatus
{
    /// <summary>
    /// The address was saved.
    /// </summary>
    Success,

    /// <summary>
    /// The address doesn't exist or doesn't belong to the customer.
    /// </summary>
    NotFound,

    /// <summary>
    /// The country isn't one of the store's countries.
    /// </summary>
    InvalidCountry,

    /// <summary>
    /// The region isn't one of the country's regions.
    /// </summary>
    InvalidRegion,
}
