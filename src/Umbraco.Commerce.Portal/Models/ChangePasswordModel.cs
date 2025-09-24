using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

public class ChangePasswordModel
{
    public Guid MemberKey { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
}
