using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.GetAll
{
    public class GetAllLibrariesHandler
    {
        private readonly LibraryContext _context;

        public GetAllLibrariesHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Library>> Handle()
        {
            return await _context.Libraries.ToListAsync();
        }
    }
}
