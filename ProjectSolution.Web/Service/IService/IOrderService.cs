using ProjectSolution.Web.Models;

namespace ProjectSolution.Web.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
        Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto);
        Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId);
        Task<ResponseDto?> GetAllOrdersAsync(string? userId);
        Task<ResponseDto?> GetOrderByIdAsync(int orderHeaderId);
        Task<ResponseDto?> UpdateOrderStatusAsync(int orderHeaderId, string newStatus);
    }
}
