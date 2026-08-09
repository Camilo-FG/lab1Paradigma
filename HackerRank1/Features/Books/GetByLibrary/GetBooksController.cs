using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.GetByLibrary
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books")]
    public class GetBooksController : ControllerBase
    {
        private readonly GetBooksHandler _handler;

        public GetBooksController(GetBooksHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int libraryId)
        {
            var books = await _handler.Handle(libraryId);
            if (books == null)
                return NotFound();
            return Ok(books);
        }
    }
}
