using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.Domain.Entities;

namespace LibraryService.Domain.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> Get(int libraryId, int[] ids);

        Task<Book> Add(Book book);

        Task<Book> Update(Book book);

        Task<bool> Delete(Book book);
    }
}
