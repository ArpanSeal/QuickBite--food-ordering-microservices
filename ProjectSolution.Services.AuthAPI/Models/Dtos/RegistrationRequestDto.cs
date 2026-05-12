namespace ProjectSolution.Services.AuthAPI.Models.Dtos
{
    public class RegistrationRequestDto
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; }
    }
}
