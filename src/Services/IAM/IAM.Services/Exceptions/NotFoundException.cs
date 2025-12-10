using System.Net;

namespace IAM.Services.Exceptions
{
    public class NotFoundException : BaseDomainException
    {
        public NotFoundException(string message = "Không tìm thấy dữ liệu")
            : base(message, HttpStatusCode.NotFound) { }
    }
}
