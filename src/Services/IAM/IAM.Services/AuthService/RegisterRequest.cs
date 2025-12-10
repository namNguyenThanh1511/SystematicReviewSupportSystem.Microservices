using IAM.Repositories.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAM.Services.AuthService
{
    public class RegisterRequest
    {
        [Required, StringLength(70)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò không được để trống")]
        public Role Role { get; set; }
    }
}
