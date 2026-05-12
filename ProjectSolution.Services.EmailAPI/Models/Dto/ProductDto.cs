namespace ProjectSolution.Services.EmailAPI.Models.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty; // ensures that ProductName is never null

        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty; //Category will define the type of product

        public string? ImageUrl { get; set; }

        public string? ImageLocalPath { get; set; }

        public IFormFile? ImageFile { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
