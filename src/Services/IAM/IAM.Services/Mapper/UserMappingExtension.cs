using IAM.Repositories.Entities;
using IAM.Services.AuthService;
using IAM.Services.Models.User;

namespace IAM.Services.Mapper
{
    public static class UserMappingExtension
    {
        public static UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = user.Role.ToString()
            };
        }

        public static User ToUserEntity(this RegisterRequest request)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true, // New users are active by default
            };
        }

    }
}
