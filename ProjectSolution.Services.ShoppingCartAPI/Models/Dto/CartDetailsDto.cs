using System.Text.Json.Serialization;

namespace ProjectSolution.Services.ShoppingCartAPI.Models.Dto
{
    public class CartDetailsDto
    {
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set; }

        [JsonIgnore]
        public CartHeaderDto? CartHeaderDto { get; set; } // Navigation Property - exposing Dto because this is a Dto class
        public int ProductId { get; set; }

        public ProductDto? ProductDto { get; set; }
        public int Count { get; set; }
    }
}
