using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Web.Models
{
    public class CouponDto
    {
        public int CouponId { get; set; }
        [Display(Name = "Coupon Code")]
        //[MaxLength(10, ErrorMessage = "Coupon code must be 10 characters or less.")]
        public required string CouponCode { get; set; }

        [Display(Name = "Discount Amount")]
        //The exception comes from using [MaxLength] on a numeric property(double). [MaxLength] expects a string/array/ICollection; applying it to double causes InvalidCastException.
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Minimum Amount")]
        public int MinAmount { get; set; }
    }
}
