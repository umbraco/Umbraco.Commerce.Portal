using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

public class AddressModel
{
    [Required]
    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string ZipCode { get; set; }

    public string? Country { get; set; }
}
