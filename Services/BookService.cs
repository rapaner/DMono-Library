using Library.Core.Data;
using Library.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class BookService : IBookService
    {
        private readonly LibraryDbContext _context;

        public BookService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        public async Task<Book?> GetCurrentBookAsync()
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .FirstOrDefaultAsync(b => b.IsCurrentlyReading);
        }

        public async Task<List<Book>> GetBooksByStatusAsync(bool isCurrentlyReading)
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .Where(b => b.IsCurrentlyReading == isCurrentlyReading)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            book.DateAdded = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var schedule = new BookReadingSchedule
            {
                BookId = book.Id,
                TargetFinishDate = book.DateAdded.AddDays(20),
                StartHour = null,
                EndHour = null
            };
            _context.BookReadingSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return book;
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<bool> DeleteBookAsync(Book book)
        {
            _context.Books.Remove(book);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task SetCurrentBookAsync(Book book)
        {
            var allBooks = await _context.Books.ToListAsync();
            foreach (var b in allBooks)
            {
                b.IsCurrentlyReading = false;
            }

            book.IsCurrentlyReading = true;

            await _context.SaveChangesAsync();
        }
    }
}
