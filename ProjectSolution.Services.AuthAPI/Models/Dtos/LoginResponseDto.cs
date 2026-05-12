namespace ProjectSolution.Services.AuthAPI.Models.Dtos
{
    public class LoginResponseDto
    {
        public UserDto? User { get; set; }
        public required string Token { get; set; }
    }
}
