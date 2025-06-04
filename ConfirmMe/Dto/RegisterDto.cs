using System.ComponentModel.DataAnnotations;

namespace ConfirmMe.Dto
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public string Role { get; set; } // Staff, HRD, Manager, Direktur
    }
}
