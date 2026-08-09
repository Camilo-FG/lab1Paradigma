using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.Domain.Entities;
using LibraryService.Domain.Repositories;

namespace LibraryService.Application.Services
{
    public class LibrariesService : ILibrariesService
    {
        private readonly ILibraryRepository _libraryRepository;

        public LibrariesService(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        public async Task<IEnumerable<Library>> Get(int[] ids)
        {
            return await _libraryRepository.Get(ids);
        }

        public async Task<Library> Add(Library library)
        {
            return await _libraryRepository.Add(library);
        }

        public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects)
        {
            return await _libraryRepository.AddRange(projects);
        }

        public async Task<Library> Update(Library library)
        {
            return await _libraryRepository.Update(library);
        }

        public async Task<bool> Delete(Library library)
        {
            // Complete the implementation
            throw new NotImplementedException();
        }
    }

    public interface ILibrariesService
    {
        Task<IEnumerable<Library>> Get(int[] ids);

        Task<Library> Add(Library library);

        Task<Library> Update(Library library);

        Task<bool> Delete(Library library);
    }
}
