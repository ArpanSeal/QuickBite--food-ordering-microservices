namespace ProjectSolution.Services.OrderAPI.Models.Dto
{
    public class CartDto
    {
        public CartHeaderDto? CartHeaderDto { get; set; }
        public IEnumerable<CartDetailsDto>? CartDetailsListDto { get; set; }
    }
}
