namespace Umbraco.Commerce.Portal.Models;

/// <summary>
/// The outcome of updating a member's account details.
/// </summary>
public enum AccountUpdateStatus
{
    /// <summary>
    /// The customer and member were updated.
    /// </summary>
    Success,

    /// <summary>
    /// Another member or customer of the store already uses the email address.
    /// </summary>
    EmailInUse,
}
