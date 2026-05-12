namespace ProjectSolution.Services.OrderAPI.Models.Dto
{
    public class OrderHeaderDto
    {
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

        public IEnumerable<OrderDetailsDto> OrderDetailsDtoList { get; set; } = new List<OrderDetailsDto>();
    }
}
