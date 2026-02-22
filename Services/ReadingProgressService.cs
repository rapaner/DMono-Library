using Library.Core.Data;
using Library.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class ReadingProgressService : IReadingProgressService
    {
        private readonly LibraryDbContext _context;

        public ReadingProgressService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<Book> AddOrUpdateReadingProgressAsync(int bookId, DateTime date, int currentPageNumber)
        {
            var book = await _context.Books
                .Include(b => b.PagesReadHistory)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            date = date.Date;

            var pagesReadBeforeDate = book.PagesReadHistory
                .Where(p => p.Date < date)
                .Sum(p => p.PagesRead);

            if (currentPageNumber <= pagesReadBeforeDate)
            {
                throw new InvalidOperationException(
                    $"Текущая страница ({currentPageNumber}) должна быть больше суммы страниц, прочитанных за предыдущие дни ({pagesReadBeforeDate})");
            }

            if (currentPageNumber > book.TotalPages)
            {
                throw new InvalidOperationException(
                    $"Текущая страница ({currentPageNumber}) не может превышать общее количество страниц ({book.TotalPages})");
            }

            var pagesReadToday = currentPageNumber - pagesReadBeforeDate;

            var existingEntry = book.PagesReadHistory.FirstOrDefault(p => p.Date == date);

            if (existingEntry != null)
            {
                existingEntry.PagesRead = pagesReadToday;
            }
            else
            {
                var newEntry = new PagesReadInDate
                {
                    BookId = bookId,
                    Date = date,
                    PagesRead = pagesReadToday
                };
                book.PagesReadHistory.Add(newEntry);
            }

            if (currentPageNumber >= book.TotalPages)
            {
                book.IsCurrentlyReading = false;
                book.DateFinished = DateTime.Now;
            }
            else if (!book.IsCurrentlyReading && currentPageNumber < book.TotalPages)
            {
                book.DateFinished = null;
            }

            await _context.SaveChangesAsync();

            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .FirstOrDefaultAsync(b => b.Id == bookId) ?? book;
        }

        public async Task<Book> RemoveReadingProgressAsync(int bookId, DateTime date)
        {
            var book = await _context.Books
                .Include(b => b.PagesReadHistory)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            date = date.Date;

            var entry = book.PagesReadHistory.FirstOrDefault(p => p.Date == date);
            if (entry != null)
            {
                _context.PagesReadHistory.Remove(entry);

                var currentPage = book.PagesReadHistory.Where(p => p.Date != date).Sum(p => p.PagesRead);
                if (currentPage < book.TotalPages)
                {
                    book.DateFinished = null;
                }

                await _context.SaveChangesAsync();
            }

            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .FirstOrDefaultAsync(b => b.Id == bookId) ?? book;
        }

        public async Task<List<PagesReadInDate>> GetReadingHistoryAsync(int bookId)
        {
            return await _context.PagesReadHistory
                .Where(p => p.BookId == bookId)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }
    }
}
