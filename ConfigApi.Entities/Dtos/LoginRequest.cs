using System.ComponentModel.DataAnnotations;

namespace ConfigApi.Entities.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "E-posta zorunlu.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunlu.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    public string Password { get; set; } = "";
}
