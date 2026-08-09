using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.GetById
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class GetLibraryController : ControllerBase
    {
        private readonly GetLibraryHandler _handler;

        public GetLibraryController(GetLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int libraryId)
        {
            var library = await _handler.Handle(libraryId);
            if (library == null)
                return NotFound();
            return Ok(library);
        }
    }
}
