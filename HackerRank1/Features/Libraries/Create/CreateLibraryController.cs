using System.Threading.Tasks;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.Create
{
    [ApiController]
    [Route("api/libraries")]
    public class CreateLibraryController : ControllerBase
    {
        private readonly CreateLibraryHandler _handler;

        public CreateLibraryController(CreateLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Add(Library l)
        {
            await _handler.Handle(l);
            return Ok(l);
        }
    }
}
