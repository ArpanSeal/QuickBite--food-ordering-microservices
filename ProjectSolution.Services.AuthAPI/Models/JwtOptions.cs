namespace ProjectSolution.Services.AuthAPI.Models
{
    public class JwtOptions
    {
        public required string Secret { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}
