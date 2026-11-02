using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Commerce.Portal.Models;

public class ChangePasswordModel
{
    [Obsolete("Account actions now act on the signed-in member; this value is ignored. Will be removed in v21.0.0")]
    public Guid MemberKey { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
}
