namespace ProjectSolution.Services.AuthAPI.Models.Dtos
{
    public class RoleRequestDto
    {
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
