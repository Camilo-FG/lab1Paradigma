using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;

namespace LibraryService.WebAPI.Features.Libraries.Create
{
    public class CreateLibraryHandler
    {
        private readonly LibraryContext _context;

        public CreateLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Library> Handle(Library library)
        {
            await _context.Libraries.AddAsync(library);

            await _context.SaveChangesAsync();
            return library;
        }
    }
}
