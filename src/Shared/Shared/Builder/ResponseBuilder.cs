using Shared.Models;
using System.Net;

namespace Shared.Builder
{
    public static class ResponseBuilder
    {
        public static ApiResponse BadRequest(string message, params string[] errors)
            => CreateError(HttpStatusCode.BadRequest, message, errors);

        public static ApiResponse Unauthorized(string message)
            => CreateError(HttpStatusCode.Unauthorized, message, message);

        public static ApiResponse Forbidden(string message)
            => CreateError(HttpStatusCode.Forbidden, message, message);

        public static ApiResponse NotFound(string message)
            => CreateError(HttpStatusCode.NotFound, message, message);

        public static ApiResponse InternalServerError(string message)
            => CreateError(HttpStatusCode.InternalServerError, message, message);

        public static ApiResponse Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            => CreateError(statusCode, message);

        private static ApiResponse CreateError(HttpStatusCode statusCode, string message, params string[] errors)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors.Length > 0
                    ? errors.Select(e => new ApiError { Message = e }).ToList()
                    : new List<ApiError> { new() { Message = message } }
            };
        }
    }
}
