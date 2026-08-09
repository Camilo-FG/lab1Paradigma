using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.Delete
{
    public class DeleteLibraryHandler
    {
        private readonly LibraryContext _context;

        public DeleteLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(int id)
        {
            var library = await _context.Libraries.FirstOrDefaultAsync(x => x.Id == id);
            if (library == null)
                return false;

            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
