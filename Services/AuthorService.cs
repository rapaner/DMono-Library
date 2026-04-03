using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly LibraryDbContext _context;

        public AuthorService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Author?> GetAuthorByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Author> AddAuthorAsync(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return author;
        }

        public async Task<Author> GetOrCreateAuthorAsync(string name)
        {
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.Name == name);

            if (author == null)
            {
                author = new Author { Name = name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
            }

            return author;
        }

        public async Task<List<AuthorWithBookCount>> GetAuthorsWithBookCountAsync()
        {
            return await _context.Authors
                .Select(a => new AuthorWithBookCount
                {
                    Id = a.Id,
                    Name = a.Name,
                    BookCount = a.Books.Count
                })
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Author> UpdateAuthorAsync(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
            return author;
        }

        public async Task<bool> DeleteAuthorAsync(Author author)
        {
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanDeleteAuthorAsync(int authorId)
        {
            return !await _context.Authors
                .Where(a => a.Id == authorId)
                .SelectMany(a => a.Books)
                .AnyAsync();
        }
    }
}
