using Library.Core.Models;

namespace Library.Services;

public interface IDateFilterService
{
    (DateTime? start, DateTime? end) GetDateRange(int filterIndex, DateTime? customStart = null, DateTime? customEnd = null);
    List<T> FilterByDateRange<T>(IEnumerable<T> items, DateTime? start, DateTime? end, Func<T, DateTime> dateSelector);
    bool IsBookInDateRange(Book book, DateTime? start, DateTime? end);
}
