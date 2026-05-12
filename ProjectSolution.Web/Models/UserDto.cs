namespace ProjectSolution.Web.Models
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
