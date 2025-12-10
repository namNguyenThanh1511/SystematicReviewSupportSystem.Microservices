using System.Net;

namespace IAM.Services.Exceptions
{
    public class BadRequestException : BaseDomainException
    {
        public BadRequestException(string message = "Yêu cầu không hợp lệ")
            : base(message, HttpStatusCode.BadRequest) { }
    }
}
