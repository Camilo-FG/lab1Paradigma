using System.Threading.Tasks;
using LibraryService.WebAPI.Infrastructure.Data;
using LibraryService.WebAPI.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.Update
{
    public class UpdateLibraryHandler
    {
        private readonly LibraryContext _context;

        public UpdateLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Library> Handle(Library library)
        {
            var projectForChanges = await _context.Libraries.SingleAsync(x => x.Id == library.Id);
            projectForChanges.Name = library.Name;
            projectForChanges.Location = library.Location;

            _context.Libraries.Update(projectForChanges);
            await _context.SaveChangesAsync();
            return library;
        }
    }
}
