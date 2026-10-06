using System.ComponentModel.DataAnnotations;

namespace ConfigApi.Entities.Dtos;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = "";
}
