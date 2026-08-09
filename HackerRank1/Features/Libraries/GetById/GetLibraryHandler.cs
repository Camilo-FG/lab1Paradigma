using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.GetById
{
    public class GetLibraryHandler
    {
        private readonly LibraryContext _context;

        public GetLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Library?> Handle(int id)
        {
            return await _context.Libraries.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
