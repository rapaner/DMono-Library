using Library.Models;

namespace Library.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsPageData> GetStatisticsPageDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? searchText = null);
        Task<List<DailyReadingData>> GetDailyReadingDataForBookAsync(int bookId, DateTime? startDate = null, DateTime? endDate = null, int? readingSessionId = null);
    }
}
