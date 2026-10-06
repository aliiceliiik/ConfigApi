using System.ComponentModel.DataAnnotations;

namespace ConfigApi.Mvc.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta zorunlu.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunlu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
