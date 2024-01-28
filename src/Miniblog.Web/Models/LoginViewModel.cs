namespace Miniblog.Web.Models;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;
}
