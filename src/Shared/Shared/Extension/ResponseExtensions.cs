using Microsoft.AspNetCore.Mvc;
using Shared.Models;


namespace Shared.Extension
{
    public static class ResponseExtensions
    {
        public static IActionResult ToApiResponse(this ControllerBase controller, object? data = null, string message = "Success")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
            return controller.Ok(response);
        }

        public static IActionResult ToErrorResponse(this ControllerBase controller, string message, params string[] errors)
        {
            var response = new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors.Select(e => new ApiError { Message = e }).ToList()
            };
            return controller.BadRequest(response);
        }

        public static IActionResult ToNotFoundResponse(this ControllerBase controller, string message = "Not found")
        {
            var response = new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<ApiError> { new() { Message = message } }
            };
            return controller.NotFound(response);
        }
    }
}
