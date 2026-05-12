namespace ProjectSolution.Services.EmailAPI.Models.Dto
{
    public class CartDto
    {
        public CartHeaderDto? CartHeaderDto { get; set; }
        public IEnumerable<CartDetailsDto>? CartDetailsListDto { get; set; }
    }
}
