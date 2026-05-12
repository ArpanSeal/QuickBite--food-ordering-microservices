using System.ComponentModel.DataAnnotations;

namespace ProjectSolution.Web.Models
{
    public class RegistrationRequestDto
    {
        [Required]           // for client-side validation asp-net-in-action page 429
        [StringLength(100)]
        [MinLength(6, ErrorMessage = "The field {0} must be with a minimum length of 6")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(50)]
        [MinLength(6, ErrorMessage = "The field {0} must be with a minimum length of 6")]
        public required string Name { get; set; }

        [Required]
        [StringLength(50)]
        [MinLength(6, ErrorMessage = "The field {0} must be with a minimum length of 6")]
        [Display(Name = "User Name")] // asp-net-in-action page 198
        public required string UserName { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The {0} must be exactly 10 characters long")]
        [Display(Name = "Phone Number")]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        [MinLength(6, ErrorMessage = "The field {0} must be with a minimum length of 6")]
        public required string Password { get; set; }
        public string? Role { get; set; }
    }
}
