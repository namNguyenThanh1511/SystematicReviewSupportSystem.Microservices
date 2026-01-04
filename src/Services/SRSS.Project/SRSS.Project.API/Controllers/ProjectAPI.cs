using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SRSS.Project.API.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectAPI() : ControllerBase
    {

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
        {
            return Ok("Project API is working!");
        }

    }
}
