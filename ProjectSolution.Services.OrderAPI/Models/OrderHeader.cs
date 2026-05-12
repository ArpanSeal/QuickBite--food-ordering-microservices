using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Services.OrderAPI.Models.Dto
{
    public class OrderHeader
    {
        [Key]
        public int OrderHeaderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public decimal OrderTotal { get; set; }


        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public string? Status { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? StripeSessionId { get; set; }

        public IEnumerable<OrderDetails> OrderDetailsList { get; set; } = new List<OrderDetails>();
    }
}
