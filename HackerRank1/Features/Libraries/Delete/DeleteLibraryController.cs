using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.Delete
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class DeleteLibraryController : ControllerBase
    {
        private readonly DeleteLibraryHandler _handler;

        public DeleteLibraryController(DeleteLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int libraryId)
        {
            var deleted = await _handler.Handle(libraryId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
