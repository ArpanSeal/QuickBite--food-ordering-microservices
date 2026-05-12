using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Web.Models
{
    public class LoginRequestDto
    {
        [Required]          // for client-side validation asp-net-in-action page 429
        [StringLength(100)]
        [MinLength(6)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100)]
        [MinLength(6, ErrorMessage = "The field {0} must be with a minimum length of 6")]
        public required string Password { get; set; }
    }
}
