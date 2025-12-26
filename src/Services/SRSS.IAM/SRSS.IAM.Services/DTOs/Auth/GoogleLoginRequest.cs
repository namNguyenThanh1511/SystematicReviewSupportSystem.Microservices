using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "Google ID Token is required")]
        public string IdToken { get; set; }
    }
}
