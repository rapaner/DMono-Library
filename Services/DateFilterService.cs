using Library.Core.Models;

namespace Library.Services;

public class DateFilterService : IDateFilterService
{
    public (DateTime? start, DateTime? end) GetDateRange(int filterIndex, DateTime? customStart = null, DateTime? customEnd = null)
    {
        return filterIndex switch
        {
            0 => (null, null),
            1 => (DateTime.Now.AddDays(-30), DateTime.Now),
            2 => (DateTime.Now.AddDays(-60), DateTime.Now),
            3 => (DateTime.Now.AddDays(-90), DateTime.Now),
            4 => (DateTime.Now.AddDays(-180), DateTime.Now),
            5 => (DateTime.Now.AddDays(-365), DateTime.Now),
            6 => (customStart, customEnd),
            _ => (null, null)
        };
    }

    public List<T> FilterByDateRange<T>(IEnumerable<T> items, DateTime? start, DateTime? end, Func<T, DateTime> dateSelector)
    {
        if (!start.HasValue && !end.HasValue)
            return items.ToList();

        return items.Where(item =>
        {
            var date = dateSelector(item);
            return (!start.HasValue || date >= start.Value.Date) &&
                   (!end.HasValue || date <= end.Value.Date);
        }).ToList();
    }

    public bool IsBookInDateRange(Book book, DateTime? start, DateTime? end)
    {
        if (!start.HasValue && !end.HasValue)
            return true;

        if (book.DateFinished.HasValue)
        {
            var dateToCheck = book.DateFinished.Value.Date;
            return (!start.HasValue || dateToCheck >= start.Value.Date) &&
                   (!end.HasValue || dateToCheck <= end.Value.Date);
        }

        if (book.PagesReadHistory.Any())
        {
            return book.PagesReadHistory.Any(p =>
                (!start.HasValue || p.Date >= start.Value.Date) &&
                (!end.HasValue || p.Date <= end.Value.Date));
        }

        return false;
    }
}
