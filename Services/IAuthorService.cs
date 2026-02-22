using Library.Core.Models;

namespace Library.Services
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAllAuthorsAsync();
        Task<Author?> GetAuthorByIdAsync(int id);
        Task<Author> AddAuthorAsync(Author author);
        Task<Author> GetOrCreateAuthorAsync(string name);
    }
}
