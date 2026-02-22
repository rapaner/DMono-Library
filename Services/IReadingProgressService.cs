using Library.Core.Models;

namespace Library.Services
{
    public interface IReadingProgressService
    {
        Task<Book> AddOrUpdateReadingProgressAsync(int bookId, DateTime date, int currentPageNumber);
        Task<Book> RemoveReadingProgressAsync(int bookId, DateTime date);
        Task<List<PagesReadInDate>> GetReadingHistoryAsync(int bookId);
    }
}
