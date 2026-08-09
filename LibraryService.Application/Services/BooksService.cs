using LibraryService.Domain.Entities;
using LibraryService.Domain.Repositories;

namespace LibraryService.Application.Services;

public class BooksService : IBooksService
{
    private readonly IBookRepository _bookRepository;

    public BooksService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Book>> Get(int libraryId, int[] ids)
    {
        return await _bookRepository.Get(libraryId, ids);
    }

    public async Task<Book> Add(Book book)
    {
        // Complete the implementation
        throw new NotImplementedException();
    }

    public async Task<Book> Update(Book book)
    {
        // Complete the implementation
        throw new NotImplementedException();
    }

    public async Task<bool> Delete(Book book)
    {
        // Complete the implementation
        throw new NotImplementedException();
    }
}

public interface IBooksService
{
    Task<IEnumerable<Book>> Get(int libraryId, int[] ids);

    Task<Book> Add(Book book);

    Task<Book> Update(Book book);

    Task<bool> Delete(Book book);
}
