using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.GetAll
{
    [ApiController]
    [Route("api/libraries")]
    public class GetAllLibrariesController : ControllerBase
    {
        private readonly GetAllLibrariesHandler _handler;

        public GetAllLibrariesController(GetAllLibrariesHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libraries = await _handler.Handle();
            return Ok(libraries);
        }
    }
}
