using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.Create
{
    public class CreateBookHandler
    {
        private readonly LibraryContext _context;

        public CreateBookHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Book?> Handle(int libraryId, BookForm form)
        {
            var libraryExists = await _context.Libraries.AnyAsync(x => x.Id == libraryId);
            if (!libraryExists)
                return null;

            var book = new Book
            {
                Name = form.Name,
                Category = form.Category ?? string.Empty,
                LibraryId = libraryId
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book;
        }
    }
}
