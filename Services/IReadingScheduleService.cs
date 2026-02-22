using Library.Core.Models;

namespace Library.Services
{
    public interface IReadingScheduleService
    {
        Task<BookReadingSchedule?> GetBookReadingScheduleAsync(int bookId);
        Task<BookReadingSchedule> UpdateBookReadingScheduleAsync(BookReadingSchedule schedule);
        Task<(int startHour, int endHour)> GetEffectiveReadingHoursAsync(int bookId);
    }
}
