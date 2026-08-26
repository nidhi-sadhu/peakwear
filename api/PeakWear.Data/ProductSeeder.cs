using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;

namespace PeakWear.Data;

public static class ProductSeeder
{
    private static readonly string[] Colours = ["Black", "Blue", "White"];
    private static readonly string[] Sizes = ["S", "M", "L"];

    public static async Task SeedAsync(PeakWearDbContext context)
    {
        if (await context.Products.AnyAsync()) return;   // idempotent

        var seed = new (string Name, string Category, string ShoppingFor, decimal Price, string Desc)[]
        {
            ("Peak Run Leggings",    "Leggings", "Women", 98.00m,  "High-rise leggings with a hidden waistband pocket."),
            ("Align Studio Leggings","Leggings", "Women", 118.00m, "Buttery-soft fabric designed for low-impact training."),
            ("Swift Training Top",   "Top",      "Women", 68.00m,  "Lightweight top with mesh ventilation panels."),
            ("Core Cropped Hoodie",  "Hoodie",   "Women", 128.00m, "Cropped fleece hoodie with a relaxed fit."),
            ("Pace Training Shorts", "Pants",    "Men",   78.00m,  "Nine-inch shorts with a zip pocket."),
            ("Metal Vent Tee",       "TShirt",   "Men",   78.00m,  "Breathable tee built for high-output sessions."),
            ("Surge Jogger",         "Pants",    "Men",   128.00m, "Tapered jogger with four-way stretch."),
            ("Steady State Hoodie",  "Hoodie",   "Men",   138.00m, "Midweight hoodie for warm-ups and rest days.")
        };

        foreach (var item in seed)
        {
            var product = new Product
            {
                Name = item.Name,
                Slug = Slugify(item.Name),
                Description = item.Desc,
                Category = item.Category,
                ShoppingFor = item.ShoppingFor,
                BasePrice = item.Price
            };

            var prefix = item.Category[..3].ToUpper();
            var random = new Random(item.Name.GetHashCode());

            foreach (var colour in Colours)
            foreach (var size in Sizes)
            {
                product.Variants.Add(new ProductVariant
                {
                    Colour = colour,
                    Size = size,
                    Sku = $"{prefix}-{colour[..3].ToUpper()}-{size}-{product.Slug[..3].ToUpper()}",
                    ImageUrl = $"/products/{product.Slug}-{colour.ToLower()}.jpg",
                    Stock = random.Next(0, 12)
                });
            }

            context.Products.Add(product);
        }

        await context.SaveChangesAsync();
    }

    private static string Slugify(string name) =>
        new string(name.ToLower().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
            .Replace("--", "-").Trim('-');
}