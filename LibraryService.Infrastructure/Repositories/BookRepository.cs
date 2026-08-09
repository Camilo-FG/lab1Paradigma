using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.Domain.Entities;
using LibraryService.Domain.Repositories;
using LibraryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Infrastructure.Repositories
{
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
}
