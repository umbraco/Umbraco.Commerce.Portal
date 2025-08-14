using System.ComponentModel.DataAnnotations;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Portal.Models;

public class CustomerModel
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Telephone { get; set; }
}
