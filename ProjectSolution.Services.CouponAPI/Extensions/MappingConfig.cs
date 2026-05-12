using ProjectSolution.Services.CouponAPI.Models;
using ProjectSolution.Services.CouponAPI.Models.Dtos;

namespace ProjectSolution.Services.CouponAPI.Extensions
{
    public static class MappingConfig
    {
        public static CouponDto ToCouponDto(this Coupon entity)
        {
            return new CouponDto
            {
                CouponId = entity.CouponId,
                CouponCode = entity.CouponCode,
                DiscountAmount = entity.DiscountAmount,
                MinAmount = entity.MinAmount
            };
        }

        public static Coupon ToCoupon(this CouponDto entity)
        {
            return new Coupon
            {
                CouponId = entity.CouponId,
                CouponCode = entity.CouponCode,
                DiscountAmount = entity.DiscountAmount,
                MinAmount = entity.MinAmount
            };
        }
    }
}
