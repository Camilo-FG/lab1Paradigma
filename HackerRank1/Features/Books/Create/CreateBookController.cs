using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.Create
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books")]
    public class CreateBookController : ControllerBase
    {
        private readonly CreateBookHandler _handler;

        public CreateBookController(CreateBookHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int libraryId, BookForm form)
        {
            var book = await _handler.Handle(libraryId, form);
            if (book == null)
                return NotFound();
            return StatusCode(StatusCodes.Status201Created, book);
        }
    }
}
