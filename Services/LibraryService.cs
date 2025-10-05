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
                .Include(b => b.Authors)
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
                .Include(b => b.Authors)
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
                .Include(b => b.Authors)
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
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // ===== Методы для работы с авторами =====

        /// <summary>
        /// Получить всех авторов из базы данных
        /// </summary>
        /// <returns>Список всех авторов</returns>
        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получить автора по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор автора</param>
        /// <returns>Автор или null, если не найден</returns>
        public async Task<Author?> GetAuthorByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Добавить нового автора
        /// </summary>
        /// <param name="author">Автор для добавления</param>
        /// <returns>Добавленный автор с присвоенным ID</returns>
        public async Task<Author> AddAuthorAsync(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return author;
        }

        /// <summary>
        /// Найти автора по имени или создать нового
        /// </summary>
        /// <param name="name">Имя автора</param>
        /// <returns>Существующий или новый автор</returns>
        public async Task<Author> GetOrCreateAuthorAsync(string name)
        {
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.Name == name);
            
            if (author == null)
            {
                author = new Author { Name = name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
            }
            
            return author;
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
            var books = await _context.Books.Include(b => b.Authors).ToListAsync();
            
            // Подсчет популярных авторов
            var authorStats = books
                .SelectMany(b => b.Authors)
                .GroupBy(a => a.Name)
                .Select(g => new { Author = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count)
                .Take(10)
                .ToList();
            
            return new LibraryStatistics
            {
                TotalBooks = books.Count,
                ReadBooks = books.Count(b => b.DateFinished.HasValue),
                CurrentBooks = books.Count(b => b.IsCurrentlyReading),
                PlannedBooks = books.Count(b => !b.IsCurrentlyReading && !b.DateFinished.HasValue),
                TotalPagesRead = books.Where(b => b.DateFinished.HasValue).Sum(b => b.TotalPages),
                PopularGenres = new List<dynamic>(), // Жанры удалены из модели
                PopularAuthors = authorStats
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
