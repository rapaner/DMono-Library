using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class ShelfService : IShelfService
    {
        private readonly LibraryDbContext _context;

        public ShelfService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Shelf>> GetAllShelvesAsync()
        {
            return await _context.Shelves
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Shelf?> GetShelfByIdAsync(int id)
        {
            return await _context.Shelves
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ShelfWithBookCount>> GetShelvesWithBookCountAsync()
        {
            return await _context.Shelves
                .OrderBy(s => s.Id)
                .Select(s => new ShelfWithBookCount
                {
                    Id = s.Id,
                    Name = s.Name,
                    BookCount = s.Books.Count
                })
                .ToListAsync();
        }

        public async Task<Shelf> AddShelfAsync(Shelf shelf)
        {
            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();
            return shelf;
        }

        public async Task<Shelf> UpdateShelfAsync(Shelf shelf)
        {
            _context.Shelves.Update(shelf);
            await _context.SaveChangesAsync();
            return shelf;
        }

        public async Task<bool> DeleteShelfAsync(Shelf shelf)
        {
            _context.Shelves.Remove(shelf);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CanDeleteShelfAsync(int shelfId)
        {
            return !await _context.Books.AnyAsync(b => b.ShelfId == shelfId);
        }
    }
}
