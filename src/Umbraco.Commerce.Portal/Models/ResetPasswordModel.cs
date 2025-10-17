using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

public class ResetPasswordModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

public class UpdatePasswordModel
{
    public Guid MemberId { get; set; }

    [Required]
    public string Password { get; set; }
}
