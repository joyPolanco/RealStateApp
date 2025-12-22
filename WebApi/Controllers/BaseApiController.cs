using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RealStateWebApi.Controllers
{
    [Route("api/v{version::apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IMediator Mediator => HttpContext!.RequestServices.GetService<IMediator>()!;
    }
}
