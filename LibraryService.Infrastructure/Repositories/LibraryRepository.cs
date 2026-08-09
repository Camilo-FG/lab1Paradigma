using LibraryService.Domain.Entities;
using LibraryService.Domain.Repositories;
using LibraryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Infrastructure.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly LibraryContext _libraryContext;

    public LibraryRepository(LibraryContext libraryContext)
    {
        _libraryContext = libraryContext;
    }

    public async Task<IEnumerable<Library>> Get(int[] ids)
    {
        var projects = _libraryContext.Libraries.AsQueryable();

        if (ids != null && ids.Any())
            projects = projects.Where(x => ids.Contains(x.Id));

        return await projects.ToListAsync();
    }

    public async Task<Library> Add(Library library)
    {
        await _libraryContext.Libraries.AddAsync(library);

        await _libraryContext.SaveChangesAsync();
        return library;
    }

    public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects)
    {
        await _libraryContext.Libraries.AddRangeAsync(projects);
        await _libraryContext.SaveChangesAsync();
        return projects;
    }

    public async Task<Library> Update(Library library)
    {
        var projectForChanges = await _libraryContext.Libraries.SingleAsync(x => x.Id == library.Id);
        projectForChanges.Name = library.Name;
        projectForChanges.Location = library.Location;

        _libraryContext.Libraries.Update(projectForChanges);
        await _libraryContext.SaveChangesAsync();
        return library;
    }

    public async Task<bool> Delete(Library library)
    {
        _libraryContext.Libraries.Remove(library);
        await _libraryContext.SaveChangesAsync();
        return true;
    }
}
