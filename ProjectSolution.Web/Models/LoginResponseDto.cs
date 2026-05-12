namespace ProjectSolution.Web.Models
{
    public class LoginResponseDto
    {
        public UserDto? User { get; set; }
        public required string Token { get; set; }
    }
}
