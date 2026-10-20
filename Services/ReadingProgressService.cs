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
            var book = await ReadingSessionHelper.WithReadingData(_context.Books)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            var session = ReadingSessionHelper.EnsureOpenSession(book, _context);
            var sessionHistory = session.PagesReadHistory;

            date = date.Date;

            var pagesReadBeforeDate = sessionHistory
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

            var existingEntry = sessionHistory.FirstOrDefault(p => p.Date == date);

            if (existingEntry != null)
            {
                existingEntry.PagesRead = pagesReadToday;
            }
            else
            {
                var newEntry = new PagesReadInDate
                {
                    BookId = bookId,
                    ReadingSessionId = session.Id,
                    Date = date,
                    PagesRead = pagesReadToday
                };
                session.PagesReadHistory.Add(newEntry);
                book.PagesReadHistory.Add(newEntry);
            }

            if (book.Status != BookStatus.Reading)
                book.Status = BookStatus.Reading;

            if (currentPageNumber >= book.TotalPages)
            {
                book.Status = BookStatus.Finished;
                book.DateFinished = DateTime.Now;
                ReadingSessionHelper.CloseSession(session);
            }
            else
            {
                if (book.Status == BookStatus.Finished)
                {
                    book.Status = BookStatus.Planned;
                    book.DateFinished = null;
                    session.FinishDate = null;
                }
            }

            await _context.SaveChangesAsync();

            return await ReloadBookAsync(bookId) ?? book;
        }

        public async Task<Book> RemoveReadingProgressAsync(int bookId, DateTime date)
        {
            var book = await ReadingSessionHelper.WithReadingData(_context.Books)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            var session = ReadingSessionHelper.GetOpenSession(book)
                ?? book.ReadingSessions.OrderByDescending(s => s.Number).FirstOrDefault();

            if (session == null)
                return book;

            date = date.Date;

            var entry = session.PagesReadHistory.FirstOrDefault(p => p.Date == date);
            if (entry != null)
            {
                _context.PagesReadHistory.Remove(entry);
                book.PagesReadHistory.Remove(entry);

                var currentPage = session.PagesReadHistory.Where(p => p.Date != date).Sum(p => p.PagesRead);
                if (currentPage < book.TotalPages)
                {
                    book.DateFinished = null;
                    session.FinishDate = null;
                    if (book.Status == BookStatus.Finished)
                        book.Status = BookStatus.Planned;
                }

                await _context.SaveChangesAsync();
            }

            return await ReloadBookAsync(bookId) ?? book;
        }

        public async Task<List<PagesReadInDate>> GetReadingHistoryAsync(int bookId, int? readingSessionId = null)
        {
            var book = await ReadingSessionHelper.WithReadingData(_context.Books)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return new List<PagesReadInDate>();

            ReadingSession? session;
            if (readingSessionId.HasValue)
            {
                session = book.ReadingSessions.FirstOrDefault(s => s.Id == readingSessionId.Value);
            }
            else
            {
                session = ReadingSessionHelper.GetOpenSession(book)
                    ?? book.ReadingSessions.OrderByDescending(s => s.Number).FirstOrDefault();
            }

            if (session == null)
                return new List<PagesReadInDate>();

            return session.PagesReadHistory.OrderBy(p => p.Date).ToList();
        }

        private async Task<Book?> ReloadBookAsync(int bookId) =>
            await ReadingSessionHelper.WithReadingData(_context.Books)
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .Include(b => b.ReadingSchedule)
                .Include(b => b.Shelf)
                .FirstOrDefaultAsync(b => b.Id == bookId);
    }
}
