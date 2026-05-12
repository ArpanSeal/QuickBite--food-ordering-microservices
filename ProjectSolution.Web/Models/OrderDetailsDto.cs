namespace ProjectSolution.Web.Models
{
    public class OrderDetailsDto
    {
        public int OrderDetailsId { get; set; }
        public int OrderHeaderId { get; set; }

        public int ProductId { get; set; }

        public ProductDto? ProductDto { get; set; }
        public int Count { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
    }
}
