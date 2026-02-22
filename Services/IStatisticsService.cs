using Library.Models;

namespace Library.Services
{
    public interface IStatisticsService
    {
        Task<LibraryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null);
        Task<List<DailyReadingData>> GetMonthlyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null);
        Task<List<DailyReadingData>> GetDailyReadingDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null);
        Task<List<DailyReadingData>> GetDailyReadingDataForBookAsync(int bookId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<BookRanking>> GetBookRankingsAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null);
    }
}
