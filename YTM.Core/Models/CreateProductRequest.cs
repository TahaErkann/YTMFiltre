using YTM.Core.Entities;

namespace YTM.Core.Models
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "Unisex";
        public List<ProductSize> Sizes { get; set; } = new List<ProductSize>();
    }
} 