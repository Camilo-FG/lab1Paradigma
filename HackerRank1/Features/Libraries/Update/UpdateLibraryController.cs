using System.Threading.Tasks;
using LibraryService.WebAPI.Features.Libraries.GetById;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.Update
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class UpdateLibraryController : ControllerBase
    {
        private readonly GetLibraryHandler _getLibraryHandler;
        private readonly UpdateLibraryHandler _updateLibraryHandler;

        public UpdateLibraryController(GetLibraryHandler getLibraryHandler, UpdateLibraryHandler updateLibraryHandler)
        {
            _getLibraryHandler = getLibraryHandler;
            _updateLibraryHandler = updateLibraryHandler;
        }

        [HttpPut]
        public async Task<IActionResult> Update(int libraryId, Library library)
        {
            var existingLibrary = await _getLibraryHandler.Handle(libraryId);
            if (existingLibrary == null)
                return NotFound();

            await _updateLibraryHandler.Handle(library);
            return NoContent();
        }
    }
}
