using IAM.Services.Configuration;

namespace IAM.Services.AuthService
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);

        Task<TokenResponse> LoginAsync(LoginRequest request);

        Task<TokenResponse> RefreshTokenAsync(string refreshToken);

        Task LogoutAsync(string userId, string accessToken, TimeSpan accessTokenTtl);
    }
}
