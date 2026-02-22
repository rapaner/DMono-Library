using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class ReadingScheduleService : IReadingScheduleService
    {
        private readonly LibraryDbContext _context;
        private readonly AppConfiguration _appConfig;

        public ReadingScheduleService(LibraryDbContext context, AppConfiguration appConfig)
        {
            _context = context;
            _appConfig = appConfig;
        }

        public async Task<BookReadingSchedule?> GetBookReadingScheduleAsync(int bookId)
        {
            return await _context.BookReadingSchedules
                .Include(s => s.Book)
                    .ThenInclude(b => b.PagesReadHistory)
                .FirstOrDefaultAsync(s => s.BookId == bookId);
        }

        public async Task<BookReadingSchedule> UpdateBookReadingScheduleAsync(BookReadingSchedule schedule)
        {
            var existing = await _context.BookReadingSchedules
                .FirstOrDefaultAsync(s => s.BookId == schedule.BookId);

            if (existing == null)
            {
                _context.BookReadingSchedules.Add(schedule);
            }
            else
            {
                existing.TargetFinishDate = schedule.TargetFinishDate;
                existing.StartHour = schedule.StartHour;
                existing.EndHour = schedule.EndHour;
            }

            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<(int startHour, int endHour)> GetEffectiveReadingHoursAsync(int bookId)
        {
            var schedule = await GetBookReadingScheduleAsync(bookId);

            int startHour = schedule?.StartHour ?? _appConfig.DefaultStartHour;
            int endHour = schedule?.EndHour ?? _appConfig.DefaultEndHour;

            return (startHour, endHour);
        }
    }
}
