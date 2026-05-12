namespace ProjectSolution.Web.Utility
{
    public class SD
    {
        public static string? CouponAPIBase { get; set; }
        public static string? AuthAPIBase { get; set; }
        public static string? ProductAPIBase { get; set; }
        public static string? ShoppingCartAPIBase { get; set; }
        public static string? OrderAPIBase { get; set; }

        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusReadyForPickup = "Ready for Pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public enum ContentType
        {
            ApplicationJson,
            MultipartFormData
        }
    }
}
