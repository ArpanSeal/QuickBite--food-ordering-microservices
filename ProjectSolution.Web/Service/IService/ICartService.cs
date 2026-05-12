using ProjectSolution.Web.Models;

namespace ProjectSolution.Web.Service.IService
{
    public interface ICartService
    {
        Task<ResponseDto?> UpsertCartAsync(CartDto cartDto);
        Task<ResponseDto?> RemoveFromCartAsync(int cartDetailId);
        Task<ResponseDto?> GetAllCartsByUserIdAsync(string userId);
        Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto);
        Task<ResponseDto?> GetCartCountByUserIdAsync(string userId);
    }
}
