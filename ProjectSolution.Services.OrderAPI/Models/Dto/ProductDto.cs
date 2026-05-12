namespace ProjectSolution.Services.OrderAPI.Models.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty; // ensures that ProductName is never null

        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty; //Category will define the type of product

        public string ImageUrl { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
    }
}
