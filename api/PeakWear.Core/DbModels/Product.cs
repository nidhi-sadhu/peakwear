using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.DbModels;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}