using System.ComponentModel.DataAnnotations;

namespace ConfigApi.Entities.Dtos;

public class ProductSaveRequest
{
    [Required(ErrorMessage = "Ürün adı zorunlu.")]
    [StringLength(250, MinimumLength = 2, ErrorMessage = "Ürün adı 2-250 karakter olmalı.")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = "";

    [StringLength(2000)]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = "";

    [Range(0.01, 9999999, ErrorMessage = "Fiyat 0'dan büyük olmalı.")]
    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "Stok negatif olamaz.")]
    [Display(Name = "Stok")]
    public int Stock { get; set; }

    [Display(Name = "Satışta")]
    public bool IsActive { get; set; } = true;
}

public class ProductAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
