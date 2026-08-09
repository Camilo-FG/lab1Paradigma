using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.GetByLibrary
{
    public class GetBooksHandler
    {
        private readonly LibraryContext _context;

        public GetBooksHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>?> Handle(int libraryId)
        {
            var libraryExists = await _context.Libraries.AnyAsync(x => x.Id == libraryId);
            if (!libraryExists)
                return null;

            var query = _context.Books.AsQueryable().Where(b => b.LibraryId == libraryId);

            return await query.ToListAsync();
        }
    }
}
