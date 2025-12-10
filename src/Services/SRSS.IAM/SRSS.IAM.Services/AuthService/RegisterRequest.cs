using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.AuthService
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email or Phone number is required")]
        [EmailOrPhoneVN]
        public string KeyRegister { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vai trò không được để trống")]
        public Role Role { get; set; }

    }
}
