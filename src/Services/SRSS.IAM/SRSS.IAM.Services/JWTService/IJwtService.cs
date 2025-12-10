using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.Configurations;

namespace SRSS.IAM.Services.JWTService
{
    public interface IJwtService
    {
        /// <summary>
        /// Tạo JWT access token
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Tạo refresh token
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(Guid userId);

        /// <summary>
        /// Validate access token
        /// </summary>
        bool ValidateAccessToken(string token);

        /// <summary>
        /// Validate refresh token
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string token);

        Task<TokenResponse> RefreshTokenAsync(string refreshToken);

        Task<bool> IsRevokeAsync(string refreshToken);
    }
}
