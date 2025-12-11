using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.CacheService;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.DTOs.Auth;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.Exceptions;
using SRSS.IAM.Services.JWTService;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.RefreshTokenService;

namespace SRSS.IAM.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IRedisService _redisService;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IJwtService jwtService, IRefreshTokenService refreshTokenService, IOptions<JwtSettings> jwtSettings, IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _jwtSettings = jwtSettings.Value;
            _redisService = redisService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            string username = request.KeyLogin;
            string email = request.KeyLogin;
            var usernameNormalized = username.Trim().ToLower();
            var emailNormalized = email.Trim().ToLower();

            var existingUser = await _unitOfWork.Users.FindSingleAsync(u =>
                u.Username.ToLower() == usernameNormalized
                || u.Email.ToLower() == emailNormalized
            );
            if (existingUser == null)
            {
                throw new NotFoundException("Người dùng không tồn tại");
            }
            if (!existingUser.IsActive)
            {
                throw new ForbiddenException("Tài khoản đã bị vô hiệu hóa");
            }

            // Verify password
            var verifyResult = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException("Mật khẩu không chính xác");
            }
            var accessToken = _jwtService.GenerateAccessToken(existingUser);
            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            return CreateLoginResponse(existingUser, accessToken);
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            // Check if username/email already exists
            var usernameNormalized = request.Username.Trim().ToLower();
            var emailNormalized = request.Email.Trim().ToLower();

            var existingUser = await _unitOfWork.Users.FindSingleAsync(u =>
                u.Username.ToLower() == usernameNormalized
                || u.Email.ToLower() == emailNormalized
            );

            if (existingUser != null)
            {
                throw new BadRequestException("Email/tên đăng nhập này đã được đăng kí");
            }


            // Create and save user
            var newUser = new User()
            {
                Username = usernameNormalized,
                FullName = request.FullName,
                Email = emailNormalized,
                Role = request.Role,
                Password = _passwordHasher.HashPassword(null, request.Password),
                IsActive = true,
            };
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task<LoginResponse> RefreshAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId)
                ?? throw new NotFoundException("Người dùng không tồn tại");
            if (user.IsActive == false)
            {
                throw new ForbiddenException("Tài khoản đã bị vô hiệu hóa");
            }
            // Generate new access token
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            return CreateLoginResponse(user, newAccessToken);
        }

        public async Task LogoutAsync(string userId, string accessToken, TimeSpan accessTokenTtl)
        {
            // 1. Revoke refresh token in DB
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == Guid.Parse(userId));
            if (user != null)
            {
                user.RefreshToken = null;
                user.IsRefreshTokenRevoked = true;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            // 2. Blacklist access token in Redis
            // Key: "blacklist:{accessToken}", Value: "revoked", Expiry: accessTokenTtl
            await _redisService.SetAsync($"blacklist:{accessToken}", "revoked", accessTokenTtl);
        }


        public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
        {
            var userGuid = Guid.Parse(userId);
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userGuid);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại");
            }
            return user.ToUserProfileResponse();

        }

        private LoginResponse CreateLoginResponse(User user, string accessToken)
        {
            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            };
        }
    }
}
