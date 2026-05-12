using ProjectSolution.Services.ShoppingCartAPI.Models.Dtos;

namespace ProjectSolution.Services.ShoppingCartAPI.Services.IService
{
    public interface ICouponService
    {
        Task<CouponDto> GetCouponByName(string couponCode);
    }
}
