using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Commerce.Portal.Models;

public class LoginModel : PostRedirectModel
{
    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(256)]
    public string Password { get; set; } = null!;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
