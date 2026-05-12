using ProjectSolution.Web.Models;

namespace ProjectSolution.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetCouponAsync(string couponCode);
        Task<ResponseDto?> GetAllCouponsAsync();
        Task<ResponseDto?> GetCouponById(int couponId);
        Task<ResponseDto?> CreateCoupon(CouponDto coupon);
        Task<ResponseDto?> UpdateCoupon(CouponDto coupon);
        Task<ResponseDto?> DeleteCouponAsync(int couponId);
    }
}
