using LibraryService.Domain.Entities;
using LibraryService.Domain.Repositories;
using LibraryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryContext _libraryContext;

    public BookRepository(LibraryContext libraryContext)
    {
        _libraryContext = libraryContext;
    }

    public async Task<IEnumerable<Book>> Get(int libraryId, int[] ids)
    {
        var query = _libraryContext.Books.AsQueryable().Where(b => b.LibraryId == libraryId);

        if (ids != null && ids.Any())
            query = query.Where(b => ids.Contains(b.Id));

        return await query.ToListAsync();
    }

    public async Task<Book> Add(Book book)
    {
        await _libraryContext.Books.AddAsync(book);
        await _libraryContext.SaveChangesAsync();
        return book;
    }

    public async Task<Book> Update(Book book)
    {
        var bookForChanges = await _libraryContext.Books.SingleAsync(x => x.Id == book.Id);
        bookForChanges.Name = book.Name;
        bookForChanges.Category = book.Category;
        bookForChanges.LibraryId = book.LibraryId;

        _libraryContext.Books.Update(bookForChanges);
        await _libraryContext.SaveChangesAsync();
        return book;
    }

    public async Task<bool> Delete(Book book)
    {
        _libraryContext.Books.Remove(book);
        await _libraryContext.SaveChangesAsync();
        return true;
    }
}
