using System.Net;

namespace IAM.Services.Exceptions
{
    public class UnauthorizedException : BaseDomainException
    {
        public UnauthorizedException(string message = "Không có quyền truy cập")
            : base(message, HttpStatusCode.Unauthorized) { }
    }
}
