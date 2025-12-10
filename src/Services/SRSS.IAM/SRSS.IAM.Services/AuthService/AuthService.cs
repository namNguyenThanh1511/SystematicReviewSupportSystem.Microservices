using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.CacheService;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.Constants;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.Exceptions;
using SRSS.IAM.Services.Helper;
using SRSS.IAM.Services.JWTService;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SRSS.IAM.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly IRedisService _redisService;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IJwtService jwtService, IOptions<JwtSettings> jwtSettings, IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
            _redisService = redisService;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            // Validate and determine login type
            //check if input is email or phone number 

            var emailAttribute = new EmailAddressAttribute();
            var isEmail = emailAttribute.IsValid(request.KeyLogin);
            var isPhone = Regex.IsMatch(request.KeyLogin, IRegexStorage.REGEX_PHONE_VN_SIMPLE);
            if (!isEmail && !isPhone)
            {
                throw new InvalidCredentialsException("Vui lòng nhập đúng định dạng email hoặc số điện thoại");
            }
            // Find user by email or phone
            User? user = null;
            if (isEmail)
            {
                user = await _unitOfWork.Users.FindSingleAsync(u => u.Email == request.KeyLogin);
            }
            else if (isPhone)
            {
                var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(request.KeyLogin);
                user = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == normalizedPhone);
            }
            // Check if user is active
            if (user == null)
                throw new NotFoundException("Tài khoản không tồn tại");
            if (!user.IsActive)
            {
                throw new InvalidCredentialsException("Tài khoản đã bị vô hiệu hóa");
            }

            // Verify password
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException("Mật khẩu không chính xác");
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id);
            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            };
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            //validate email format and phoneNumber
            bool isEmail = ValidationHelper.IsEmail(request.KeyRegister);
            bool isPhone = ValidationHelper.IsPhoneNumber(request.KeyRegister) && ValidationHelper.IsVietnamesePhone(request.KeyRegister);
            bool isCreateAccount = false;
            if (!isEmail && !isPhone)
            {
                throw new BadRequestException("Tài khoản không đúng định dạng email hoặc số điện thoại");
            }
            string? email = null;
            string? phoneNumber = null;
            if (isEmail)
            {
                email = request.KeyRegister;
                var existingEmailUser = await _unitOfWork.Users.FindSingleAsync(u => u.Email == email);

                if (existingEmailUser != null)
                {
                    //if (!existingEmailUser.isVerified)//đã tạo account nhưng chưa verify qua otp
                    //{
                    //    try
                    //    {
                    //        await RequestOTPDbAsync(email);

                    //    }
                    //    catch (OTPRequiredException e)
                    //    {
                    //        throw new OTPRequiredException(e.Message, "email");
                    //    }
                    //}
                    //else //trùng email với user khác
                    //{
                    //    throw new Exception("Email này đã được đăng kí");
                    //}
                    throw new BadRequestException("Email này đã được đăng kí");
                }
                else
                {
                    //await SendEmailOTPDb(email, model.fullName, null);
                    isCreateAccount = true;
                }

            }
            else if (isPhone)
            {
                phoneNumber = ValidationHelper.CleanPhoneForDB(request.KeyRegister);
                var existingPhoneUser = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == phoneNumber);
                if (existingPhoneUser != null)
                {
                    //if (!existingPhoneUser.isVerified)//đã tạo account nhưng chưa verify qua otp
                    //{
                    //    try
                    //    {
                    //        await RequestOTPDbAsync(phoneNumber);

                    //    }
                    //    catch (OTPRequiredException e)
                    //    {
                    //        throw new OTPRequiredException(e.Message, "phone");
                    //    }
                    //}
                    //else//trùng email với user khác
                    //{
                    //    throw new Exception("Số điện thoại này đã được đăng kí");
                    //}
                    throw new BadRequestException("Số điện thoại này đã được đăng kí");
                }
                else
                {
                    //await _zaloApiService.SendOTPDbAsync(phoneNumber, null, false);
                    isCreateAccount = true;
                }
            }
            else
            {
                throw new BadRequestException("Không đúng định dạng email hoặc số điện thoại");
            }

            if (isCreateAccount)
            {
                // Create and save user
                var newUser = new User()
                {
                    FullName = request.FullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Role = request.Role,
                    Password = _passwordHasher.HashPassword(null, request.Password),
                    IsActive = true,
                };
                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
            }
        }


        public Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            return _jwtService.RefreshTokenAsync(refreshToken);
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
            return new UserProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
            };

        }
    }
}
