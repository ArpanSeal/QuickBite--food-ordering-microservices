using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Services.ProductAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, 10000)]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; //Category will define the type of product
        public string ImageUrl { get; set; } = string.Empty;
        public string? ImageLocalPath { get; set; }
    }
}
