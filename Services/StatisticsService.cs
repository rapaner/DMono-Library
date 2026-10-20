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

            var filteredBooks = books;
            if (startDate.HasValue || endDate.HasValue)
            {
                filteredBooks = books.Where(b => _dateFilter.IsBookInDateRange(b, startDate, endDate)).ToList();
            }

            var authorStats = filteredBooks
                .SelectMany(b => b.Authors)
                .GroupBy(a => a.Name)
                .Select(g => new AuthorStatistic { Author = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count)
                .Take(10)
                .ToList();

            int totalPagesRead = 0;
            foreach (var book in filteredBooks)
            {
                if (startDate.HasValue || endDate.HasValue)
                {
                    totalPagesRead += _dateFilter
                        .FilterByDateRange(book.PagesReadHistory, startDate, endDate, p => p.Date)
                        .Sum(p => p.PagesRead);
                }
                else
                {
                    totalPagesRead += book.PagesReadHistory.Sum(p => p.PagesRead);
                }
            }

            var allSessions = filteredBooks.SelectMany(b => b.ReadingSessions).ToList();
            var completedSessions = allSessions.Where(s => s.FinishDate != null).ToList();

            if (startDate.HasValue || endDate.HasValue)
            {
                completedSessions = completedSessions.Where(s =>
                {
                    var finish = s.FinishDate!.Value.Date;
                    return (!startDate.HasValue || finish >= startDate.Value.Date) &&
                           (!endDate.HasValue || finish <= endDate.Value.Date);
                }).ToList();
            }

            var rereadBooks = filteredBooks.Count(b => b.ReadingSessions.Count(s => s.FinishDate != null) >= 2);

            return new LibraryStatistics
            {
                TotalBooks = filteredBooks.Count,
                ReadBooks = filteredBooks.Count(b => b.Status == BookStatus.Finished || b.Status == BookStatus.FinishedLongAgo),
                CurrentBooks = filteredBooks.Count(b => b.Status == BookStatus.Reading),
                PlannedBooks = filteredBooks.Count(b => b.Status == BookStatus.Planned),
                WontReadBooks = filteredBooks.Count(b => b.Status == BookStatus.WontRead),
                TotalPagesRead = totalPagesRead,
                TotalReadings = completedSessions.Count,
                RereadBooks = rereadBooks,
                PopularAuthors = authorStats
            };
        }

        public async Task<List<DailyReadingData>> GetMonthlyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await FilterBooksAsync(searchText);
            var allReadingHistory = await GetFilteredHistoryForBooksAsync(books, startDate, endDate);

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
            var books = await FilterBooksAsync(searchText);
            var allReadingHistory = await GetFilteredHistoryForBooksAsync(books, startDate, endDate);

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
            var books = await ReadingSessionHelper.WithReadingData(_context.Books)
                .Include(b => b.Authors)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    b.Authors.Any(a => a.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var rankings = new List<BookRanking>();

            foreach (var book in books)
            {
                foreach (var session in book.ReadingSessions.Where(s => s.PagesReadHistory.Any()))
                {
                    var history = session.PagesReadHistory.AsEnumerable();
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        history = _dateFilter.FilterByDateRange(history.ToList(), startDate, endDate, p => p.Date);
                        if (!history.Any())
                            continue;
                    }

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

        private async Task<List<Book>> FilterBooksAsync(string? searchText)
        {
            var books = await _context.Books
                .Include(b => b.Authors)
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

        private async Task<List<PagesReadInDate>> GetFilteredHistoryForBooksAsync(List<Book> books, DateTime? startDate, DateTime? endDate)
        {
            var bookIds = books.Select(b => b.Id).ToList();
            var allReadingHistory = await _context.PagesReadHistory
                .Where(p => bookIds.Contains(p.BookId))
                .OrderBy(p => p.Date)
                .ToListAsync();

            return _dateFilter.FilterByDateRange(allReadingHistory, startDate, endDate, p => p.Date);
        }
    }
}
