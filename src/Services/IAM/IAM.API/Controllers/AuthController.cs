using IAM.Services.AuthService;
using Microsoft.AspNetCore.Mvc;
using Shared.Extension;
using Shared.Models;

namespace IAM.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Thông báo đăng ký thành công</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authService.RegisterAsync(request);


                return this.ToApiResponse(
                    data: new
                    {
                        email = request.Email,
                        fullName = request.FullName,
                        role = request.Role.ToString()
                    },
                    message: "Đăng ký tài khoản thành công"
                );
            }
            catch (Exception ex)
            {
                return this.ToErrorResponse("Đăng ký thất bại", ex.Message);
            }
        }

        /// <summary>
        /// Đăng nhập vào hệ thống
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Thông tin phiên đăng nhập</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var tokenResponse = await _authService.LoginAsync(request);
                return this.ToApiResponse(
                    new
                    {
                        accessToken = tokenResponse.AccessToken,
                        refreshToken = tokenResponse.RefreshToken,
                        tokenType = tokenResponse.TokenType,
                        accessTokenExpiry = tokenResponse.AccessTokenExpiry,
                        refreshTokenExpiry = tokenResponse.RefreshTokenExpiry
                    },
                    "Login successful");
            }
            catch (Exception ex)
            {
                return this.ToErrorResponse("Login failed", ex.Message);
            }
        }
    }
}
