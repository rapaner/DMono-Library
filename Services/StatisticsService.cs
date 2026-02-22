using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly LibraryDbContext _context;

        public StatisticsService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<LibraryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await _context.Books
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
                filteredBooks = books.Where(b =>
                {
                    if (b.DateFinished.HasValue)
                    {
                        var dateToCheck = b.DateFinished.Value.Date;
                        return (!startDate.HasValue || dateToCheck >= startDate.Value.Date) &&
                               (!endDate.HasValue || dateToCheck <= endDate.Value.Date);
                    }

                    if (b.PagesReadHistory.Any())
                    {
                        return b.PagesReadHistory.Any(p =>
                            (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                            (!endDate.HasValue || p.Date <= endDate.Value.Date));
                    }

                    return false;
                }).ToList();
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
                    totalPagesRead += book.PagesReadHistory
                        .Where(p => (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                                   (!endDate.HasValue || p.Date <= endDate.Value.Date))
                        .Sum(p => p.PagesRead);
                }
                else
                {
                    totalPagesRead += book.CurrentPage;
                }
            }

            return new LibraryStatistics
            {
                TotalBooks = filteredBooks.Count,
                ReadBooks = filteredBooks.Count(b => b.Status == BookStatus.Finished),
                CurrentBooks = filteredBooks.Count(b => b.Status == BookStatus.Reading),
                PlannedBooks = filteredBooks.Count(b => b.Status == BookStatus.Planned),
                TotalPagesRead = totalPagesRead,
                PopularAuthors = authorStats
            };
        }

        public async Task<List<DailyReadingData>> GetMonthlyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null)
        {
            var books = await _context.Books
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

            var bookIds = books.Select(b => b.Id).ToList();
            var allReadingHistory = await _context.PagesReadHistory
                .Where(p => bookIds.Contains(p.BookId))
                .OrderBy(p => p.Date)
                .ToListAsync();

            if (startDate.HasValue || endDate.HasValue)
            {
                allReadingHistory = allReadingHistory
                    .Where(p => (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                               (!endDate.HasValue || p.Date <= endDate.Value.Date))
                    .ToList();
            }

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
            var books = await _context.Books
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

            var bookIds = books.Select(b => b.Id).ToList();
            var allReadingHistory = await _context.PagesReadHistory
                .Where(p => bookIds.Contains(p.BookId))
                .OrderBy(p => p.Date)
                .ToListAsync();

            if (startDate.HasValue || endDate.HasValue)
            {
                allReadingHistory = allReadingHistory
                    .Where(p => (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                               (!endDate.HasValue || p.Date <= endDate.Value.Date))
                    .ToList();
            }

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

        public async Task<List<DailyReadingData>> GetDailyReadingDataForBookAsync(int bookId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var bookReadingHistory = await _context.PagesReadHistory
                .Where(p => p.BookId == bookId)
                .OrderBy(p => p.Date)
                .ToListAsync();

            if (startDate.HasValue || endDate.HasValue)
            {
                bookReadingHistory = bookReadingHistory
                    .Where(p => (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                               (!endDate.HasValue || p.Date <= endDate.Value.Date))
                    .ToList();
            }

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
            var books = await _context.Books
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

            var booksWithHistory = books.Where(b => b.PagesReadHistory.Any()).ToList();

            if (startDate.HasValue || endDate.HasValue)
            {
                booksWithHistory = booksWithHistory.Where(b =>
                    b.PagesReadHistory.Any(p =>
                        (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                        (!endDate.HasValue || p.Date <= endDate.Value.Date))
                ).ToList();
            }

            var rankings = booksWithHistory.Select(b =>
            {
                var totalPagesRead = b.PagesReadHistory.Sum(p => p.PagesRead);
                var uniqueDaysCount = b.PagesReadHistory.Select(p => p.Date.Date).Distinct().Count();
                var averagePagesPerDay = uniqueDaysCount > 0 ? (double)totalPagesRead / uniqueDaysCount : 0;

                return new BookRanking
                {
                    BookId = b.Id,
                    Title = b.Title,
                    AuthorsText = b.AuthorsText,
                    TotalPagesRead = totalPagesRead,
                    UniqueDaysCount = uniqueDaysCount,
                    AveragePagesPerDay = averagePagesPerDay
                };
            })
            .OrderByDescending(r => r.AveragePagesPerDay)
            .ThenByDescending(r => r.TotalPagesRead)
            .ToList();

            return rankings;
        }
    }
}
