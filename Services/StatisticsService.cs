using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly LibraryDbContext _context;
        private readonly IDateFilterService _dateFilter;

        public StatisticsService(LibraryDbContext context, IDateFilterService dateFilter)
        {
            _context = context;
            _dateFilter = dateFilter;
        }

        public async Task<LibraryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await LoadBooksWithReadingDataAsync(searchText);
            var hasPeriod = startDate.HasValue || endDate.HasValue;

            if (!hasPeriod)
            {
                var authorStats = books
                    .SelectMany(b => b.Authors)
                    .GroupBy(a => a.Name)
                    .Select(g => new AuthorStatistic { Author = g.Key, Count = g.Count() })
                    .OrderByDescending(a => a.Count)
                    .Take(10)
                    .ToList();

                var totalPagesRead = books.Sum(b => b.PagesReadHistory.Sum(p => p.PagesRead));
                var completedSessions = books
                    .SelectMany(b => b.ReadingSessions)
                    .Where(s => s.FinishDate != null)
                    .ToList();

                return new LibraryStatistics
                {
                    TotalBooks = books.Count,
                    ReadBooks = books.Count(b => b.Status == BookStatus.Finished || b.Status == BookStatus.FinishedLongAgo),
                    CurrentBooks = books.Count(b => b.Status == BookStatus.Reading),
                    PlannedBooks = books.Count(b => b.Status == BookStatus.Planned),
                    WontReadBooks = books.Count(b => b.Status == BookStatus.WontRead),
                    TotalPagesRead = totalPagesRead,
                    TotalReadings = completedSessions.Count,
                    RereadBooks = books.Count(b => b.ReadingSessions.Count(s => s.FinishDate != null) >= 2),
                    PopularAuthors = authorStats
                };
            }

            var selectedSessions = SelectSessionsInPeriod(books, startDate, endDate);
            var bookById = books.ToDictionary(b => b.Id);
            var booksInScope = selectedSessions
                .Select(s => bookById[s.BookId])
                .DistinctBy(b => b.Id)
                .ToList();

            var authorStatsPeriod = booksInScope
                .SelectMany(b => b.Authors)
                .GroupBy(a => a.Name)
                .Select(g => new AuthorStatistic { Author = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count)
                .Take(10)
                .ToList();

            var totalPagesReadPeriod = selectedSessions.Sum(s => s.PagesReadHistory.Sum(p => p.PagesRead));
            var completedInScope = selectedSessions.Count(s => s.FinishDate != null);

            return new LibraryStatistics
            {
                TotalBooks = booksInScope.Count,
                ReadBooks = booksInScope.Count(b => b.Status == BookStatus.Finished || b.Status == BookStatus.FinishedLongAgo),
                CurrentBooks = booksInScope.Count(b => b.Status == BookStatus.Reading),
                PlannedBooks = booksInScope.Count(b => b.Status == BookStatus.Planned),
                WontReadBooks = booksInScope.Count(b => b.Status == BookStatus.WontRead),
                TotalPagesRead = totalPagesReadPeriod,
                TotalReadings = completedInScope,
                RereadBooks = selectedSessions.Where(s => s.Number >= 2).Select(s => s.BookId).Distinct().Count(),
                PopularAuthors = authorStatsPeriod
            };
        }

        public async Task<List<DailyReadingData>> GetMonthlyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await LoadBooksWithReadingDataAsync(searchText);
            var allReadingHistory = await GetReadingHistoryForStatisticsAsync(books, startDate, endDate);

            var monthlyData = allReadingHistory
                .GroupBy(p => new { p.Date.Year, p.Date.Month })
                .Select(g => new DailyReadingData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    PagesRead = g.Sum(p => p.PagesRead)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return monthlyData;
        }

        public async Task<List<DailyReadingData>> GetDailyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await LoadBooksWithReadingDataAsync(searchText);
            var allReadingHistory = await GetReadingHistoryForStatisticsAsync(books, startDate, endDate);

            var dailyData = allReadingHistory
                .GroupBy(p => p.Date.Date)
                .Select(g => new DailyReadingData
                {
                    Date = g.Key,
                    PagesRead = g.Sum(p => p.PagesRead)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return dailyData;
        }

        public async Task<List<DailyReadingData>> GetDailyReadingDataForBookAsync(int bookId, DateTime? startDate = null, DateTime? endDate = null, int? readingSessionId = null)
        {
            IQueryable<PagesReadInDate> query = _context.PagesReadHistory.Where(p => p.BookId == bookId);

            if (readingSessionId.HasValue)
                query = query.Where(p => p.ReadingSessionId == readingSessionId.Value);

            var bookReadingHistory = await query.OrderBy(p => p.Date).ToListAsync();

            bookReadingHistory = _dateFilter.FilterByDateRange(bookReadingHistory, startDate, endDate, p => p.Date);

            var dailyData = bookReadingHistory
                .GroupBy(p => p.Date.Date)
                .Select(g => new DailyReadingData
                {
                    Date = g.Key,
                    PagesRead = g.Sum(p => p.PagesRead)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return dailyData;
        }

        public async Task<List<BookRanking>> GetBookRankingsAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await LoadBooksWithReadingDataAsync(searchText);
            var hasPeriod = startDate.HasValue || endDate.HasValue;
            var rankings = new List<BookRanking>();

            foreach (var book in books)
            {
                foreach (var session in book.ReadingSessions.Where(s => s.PagesReadHistory.Any()))
                {
                    if (hasPeriod && !_dateFilter.IsReadingSessionInDateRange(session, startDate, endDate))
                        continue;

                    var history = session.PagesReadHistory;
                    var totalPagesRead = history.Sum(p => p.PagesRead);
                    var uniqueDaysCount = history.Select(p => p.Date.Date).Distinct().Count();
                    var averagePagesPerDay = uniqueDaysCount > 0 ? (double)totalPagesRead / uniqueDaysCount : 0;

                    rankings.Add(new BookRanking
                    {
                        BookId = book.Id,
                        ReadingSessionId = session.Id,
                        ReadingNumber = session.Number,
                        ReadingLabel = session.Number > 1 ? $"×{session.Number}" : string.Empty,
                        Title = book.Title,
                        AuthorsText = book.AuthorsText,
                        TotalPagesRead = totalPagesRead,
                        UniqueDaysCount = uniqueDaysCount,
                        AveragePagesPerDay = averagePagesPerDay
                    });
                }
            }

            rankings = rankings
                .OrderByDescending(r => r.AveragePagesPerDay)
                .ThenByDescending(r => r.TotalPagesRead)
                .ToList();

            for (var i = 0; i < rankings.Count; i++)
                rankings[i].Rank = i + 1;

            return rankings;
        }

        private async Task<List<Book>> LoadBooksWithReadingDataAsync(string? searchText)
        {
            var books = await ReadingSessionHelper.WithReadingData(_context.Books)
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    b.Authors.Any(a => a.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return books;
        }

        private List<ReadingSession> SelectSessionsInPeriod(List<Book> books, DateTime? startDate, DateTime? endDate)
        {
            return books
                .SelectMany(b => b.ReadingSessions)
                .Where(s => _dateFilter.IsReadingSessionInDateRange(s, startDate, endDate))
                .ToList();
        }

        private Task<List<PagesReadInDate>> GetReadingHistoryForStatisticsAsync(List<Book> books, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue && !endDate.HasValue)
            {
                var bookIds = books.Select(b => b.Id).ToList();
                return _context.PagesReadHistory
                    .Where(p => bookIds.Contains(p.BookId))
                    .OrderBy(p => p.Date)
                    .ToListAsync();
            }

            var history = SelectSessionsInPeriod(books, startDate, endDate)
                .SelectMany(s => s.PagesReadHistory)
                .OrderBy(p => p.Date)
                .ToList();

            return Task.FromResult(history);
        }
    }
}
