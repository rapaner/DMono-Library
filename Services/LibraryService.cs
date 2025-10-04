using Microsoft.EntityFrameworkCore;
using Library.Models;
using Library.Data;
using System.Collections.ObjectModel;

namespace Library.Services
{
    /// <summary>
    /// Сервис для работы с библиотекой книг через Entity Framework Core
    /// </summary>
    public class LibraryService
    {
        private readonly LibraryDbContext _context;

        /// <summary>
        /// Конструктор сервиса библиотеки
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public LibraryService(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все книги из библиотеки, отсортированные по дате добавления
        /// </summary>
        /// <returns>Список всех книг</returns>
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Получить текущую читаемую книгу
        /// </summary>
        /// <returns>Текущая книга или null, если нет активной книги</returns>
        public async Task<Book?> GetCurrentBookAsync()
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.IsCurrentlyReading);
        }

        /// <summary>
        /// Получить книги по статусу чтения
        /// </summary>
        /// <param name="isCurrentlyReading">Статус чтения</param>
        /// <returns>Список книг с указанным статусом</returns>
        public async Task<List<Book>> GetBooksByStatusAsync(bool isCurrentlyReading)
        {
            return await _context.Books
                .Where(b => b.IsCurrentlyReading == isCurrentlyReading)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Получить книгу по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор книги</param>
        /// <returns>Книга или null, если не найдена</returns>
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Добавить новую книгу в библиотеку
        /// </summary>
        /// <param name="book">Книга для добавления</param>
        /// <returns>Добавленная книга с присвоенным ID</returns>
        public async Task<Book> AddBookAsync(Book book)
        {
            book.DateAdded = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        /// <summary>
        /// Обновить существующую книгу
        /// </summary>
        /// <param name="book">Книга для обновления</param>
        /// <returns>Обновленная книга</returns>
        public async Task<Book> UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            return book;
        }

        /// <summary>
        /// Удалить книгу из библиотеки
        /// </summary>
        /// <param name="book">Книга для удаления</param>
        /// <returns>True, если книга была удалена</returns>
        public async Task<bool> DeleteBookAsync(Book book)
        {
            _context.Books.Remove(book);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Установить книгу как текущую читаемую
        /// </summary>
        /// <param name="book">Книга для установки как текущая</param>
        /// <returns>Задача асинхронной операции</returns>
        public async Task SetCurrentBookAsync(Book book)
        {
            // Сбросить флаг текущей книги у всех книг
            var allBooks = await _context.Books.ToListAsync();
            foreach (var b in allBooks)
            {
                b.IsCurrentlyReading = false;
            }

            // Установить новую текущую книгу
            book.IsCurrentlyReading = true;
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновить прогресс чтения книги
        /// </summary>
        /// <param name="book">Книга для обновления прогресса</param>
        /// <param name="currentPage">Текущая страница</param>
        /// <returns>Обновленная книга</returns>
        public async Task<Book> UpdateProgressAsync(Book book, int currentPage)
        {
            book.CurrentPage = currentPage;
            
            if (currentPage >= book.TotalPages && book.TotalPages > 0)
            {
                book.IsCurrentlyReading = false;
                book.DateFinished = DateTime.Now;
            }
            
            await _context.SaveChangesAsync();
            return book;
        }

        /// <summary>
        /// Получить статистику библиотеки
        /// </summary>
        /// <returns>Статистика библиотеки</returns>
        public async Task<LibraryStatistics> GetStatisticsAsync()
        {
            var books = await _context.Books.ToListAsync();
            
            return new LibraryStatistics
            {
                TotalBooks = books.Count,
                ReadBooks = books.Count(b => b.DateFinished.HasValue),
                CurrentBooks = books.Count(b => b.IsCurrentlyReading),
                PlannedBooks = books.Count(b => !b.IsCurrentlyReading && !b.DateFinished.HasValue),
                TotalPagesRead = books.Where(b => b.DateFinished.HasValue).Sum(b => b.TotalPages),
                PopularGenres = books
                    .Where(b => !string.IsNullOrEmpty(b.Genre))
                    .GroupBy(b => b.Genre)
                    .Select(g => new { Genre = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToList(),
                PopularAuthors = books
                    .Where(b => !string.IsNullOrEmpty(b.Author))
                    .GroupBy(b => b.Author)
                    .Select(g => new { Author = g.Key, Count = g.Count() })
                    .OrderByDescending(a => a.Count)
                    .Take(10)
                    .ToList()
            };
        }

        /// <summary>
        /// Проверить и применить миграции базы данных
        /// </summary>
        /// <returns>Задача асинхронной операции</returns>
        public async Task EnsureDatabaseCreatedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
        }
    }

    /// <summary>
    /// Статистика библиотеки
    /// </summary>
    public class LibraryStatistics
    {
        /// <summary>
        /// Общее количество книг
        /// </summary>
        public int TotalBooks { get; set; }

        /// <summary>
        /// Количество прочитанных книг
        /// </summary>
        public int ReadBooks { get; set; }

        /// <summary>
        /// Количество книг, читаемых сейчас
        /// </summary>
        public int CurrentBooks { get; set; }

        /// <summary>
        /// Количество книг в планах
        /// </summary>
        public int PlannedBooks { get; set; }

        /// <summary>
        /// Общее количество прочитанных страниц
        /// </summary>
        public int TotalPagesRead { get; set; }

        /// <summary>
        /// Популярные жанры
        /// </summary>
        public IEnumerable<dynamic> PopularGenres { get; set; } = new List<dynamic>();

        /// <summary>
        /// Популярные авторы
        /// </summary>
        public IEnumerable<dynamic> PopularAuthors { get; set; } = new List<dynamic>();
    }
}
