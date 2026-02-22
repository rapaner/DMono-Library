using Library.Core.Models;

namespace Library.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetCurrentBookAsync();
        Task<List<Book>> GetBooksByStatusAsync(bool isCurrentlyReading);
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book> AddBookAsync(Book book);
        Task<Book> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(Book book);
        Task SetCurrentBookAsync(Book book);
    }
}
