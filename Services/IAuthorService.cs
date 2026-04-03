using Library.Core.Models;
using Library.Models;

namespace Library.Services
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAllAuthorsAsync();
        Task<Author?> GetAuthorByIdAsync(int id);
        Task<Author> AddAuthorAsync(Author author);
        Task<Author> GetOrCreateAuthorAsync(string name);
        Task<List<AuthorWithBookCount>> GetAuthorsWithBookCountAsync();
        Task<Author> UpdateAuthorAsync(Author author);
        Task<bool> DeleteAuthorAsync(Author author);
        Task<bool> CanDeleteAuthorAsync(int authorId);
    }
}
