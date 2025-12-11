using System.Net;

namespace SRSS.IAM.Services.Exceptions
{
    public class ForbiddenException : BaseDomainException
    {
        public ForbiddenException(string message = "Bị cấm truy cập")
            : base(message, HttpStatusCode.Forbidden, "FORBIDDEN") { }
    }
}
