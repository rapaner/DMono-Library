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
                .Include(b => b.PagesReadHistory)
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
                .Include(b => b.PagesReadHistory)
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
                .Include(b => b.PagesReadHistory)
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
                .Include(b => b.PagesReadHistory)
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
        /// Добавить или обновить запись о прочитанных страницах за определенную дату
        /// </summary>
        /// <param name="bookId">Идентификатор книги</param>
        /// <param name="date">Дата чтения</param>
        /// <param name="currentPageNumber">Номер страницы, на которой сейчас находится читатель</param>
        /// <returns>Обновленная книга</returns>
        /// <exception cref="InvalidOperationException">Если текущая страница меньше или равна сумме предыдущих страниц</exception>
        public async Task<Book> AddOrUpdateReadingProgressAsync(int bookId, DateTime date, int currentPageNumber)
        {
            var book = await _context.Books
                .Include(b => b.PagesReadHistory)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            // Убираем время из даты, оставляем только дату
            date = date.Date;

            // Вычисляем сумму страниц, прочитанных до указанной даты
            var pagesReadBeforeDate = book.PagesReadHistory
                .Where(p => p.Date < date)
                .Sum(p => p.PagesRead);

            // Проверяем, что текущая страница больше суммы предыдущих
            if (currentPageNumber <= pagesReadBeforeDate)
            {
                throw new InvalidOperationException(
                    $"Текущая страница ({currentPageNumber}) должна быть больше суммы страниц, прочитанных за предыдущие дни ({pagesReadBeforeDate})");
            }

            // Проверяем, что не превышаем общее количество страниц
            if (currentPageNumber > book.TotalPages)
            {
                throw new InvalidOperationException(
                    $"Текущая страница ({currentPageNumber}) не может превышать общее количество страниц ({book.TotalPages})");
            }

            // Вычисляем количество страниц, прочитанных за этот день
            var pagesReadToday = currentPageNumber - pagesReadBeforeDate;

            // Ищем существующую запись за эту дату
            var existingEntry = book.PagesReadHistory.FirstOrDefault(p => p.Date == date);

            if (existingEntry != null)
            {
                // Обновляем существующую запись
                existingEntry.PagesRead = pagesReadToday;
            }
            else
            {
                // Создаем новую запись
                var newEntry = new PagesReadInDate
                {
                    BookId = bookId,
                    Date = date,
                    PagesRead = pagesReadToday
                };
                book.PagesReadHistory.Add(newEntry);
            }

            // Обновляем статус книги
            if (currentPageNumber >= book.TotalPages)
            {
                book.IsCurrentlyReading = false;
                book.DateFinished = DateTime.Now;
            }
            else if (!book.IsCurrentlyReading && currentPageNumber < book.TotalPages)
            {
                // Если книга была завершена, но прогресс обновлен на меньшее значение
                book.DateFinished = null;
            }

            await _context.SaveChangesAsync();
            
            // Перезагружаем книгу с обновленными данными
            return await GetBookByIdAsync(bookId) ?? book;
        }

        /// <summary>
        /// Удалить запись о прочитанных страницах за определенную дату
        /// </summary>
        /// <param name="bookId">Идентификатор книги</param>
        /// <param name="date">Дата записи для удаления</param>
        /// <returns>Обновленная книга</returns>
        public async Task<Book> RemoveReadingProgressAsync(int bookId, DateTime date)
        {
            var book = await _context.Books
                .Include(b => b.PagesReadHistory)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new InvalidOperationException("Книга не найдена");

            date = date.Date;

            var entry = book.PagesReadHistory.FirstOrDefault(p => p.Date == date);
            if (entry != null)
            {
                _context.PagesReadHistory.Remove(entry);
                
                // Обновляем статус книги
                var currentPage = book.PagesReadHistory.Where(p => p.Date != date).Sum(p => p.PagesRead);
                if (currentPage < book.TotalPages)
                {
                    book.DateFinished = null;
                }
                
                await _context.SaveChangesAsync();
            }

            return await GetBookByIdAsync(bookId) ?? book;
        }

        /// <summary>
        /// Получить историю чтения книги, отсортированную по дате
        /// </summary>
        /// <param name="bookId">Идентификатор книги</param>
        /// <returns>Список записей о прочитанных страницах</returns>
        public async Task<List<PagesReadInDate>> GetReadingHistoryAsync(int bookId)
        {
            return await _context.PagesReadHistory
                .Where(p => p.BookId == bookId)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Получить статистику библиотеки
        /// </summary>
        /// <param name="startDate">Начальная дата периода (null для всего времени)</param>
        /// <param name="endDate">Конечная дата периода (null для всего времени)</param>
        /// <returns>Статистика библиотеки</returns>
        public async Task<LibraryStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.PagesReadHistory)
                .ToListAsync();
            
            // Фильтрация книг по датам (для статуса "Прочитано")
            var filteredBooks = books;
            if (startDate.HasValue || endDate.HasValue)
            {
                filteredBooks = books.Where(b =>
                {
                    // Для прочитанных книг проверяем DateFinished
                    if (b.DateFinished.HasValue)
                    {
                        var dateToCheck = b.DateFinished.Value.Date;
                        return (!startDate.HasValue || dateToCheck >= startDate.Value.Date) &&
                               (!endDate.HasValue || dateToCheck <= endDate.Value.Date);
                    }
                    
                    // Для других книг проверяем, есть ли записи о чтении в указанный период
                    if (b.PagesReadHistory.Any())
                    {
                        return b.PagesReadHistory.Any(p =>
                            (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                            (!endDate.HasValue || p.Date <= endDate.Value.Date));
                    }
                    
                    // Книги без истории чтения и без даты завершения
                    return false;
                }).ToList();
            }
            
            // Подсчет популярных авторов на основе отфильтрованных книг
            var authorStats = filteredBooks
                .SelectMany(b => b.Authors)
                .GroupBy(a => a.Name)
                .Select(g => new AuthorStatistic { Author = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count)
                .Take(10)
                .ToList();
            
            // Подсчет страниц с учетом фильтра по датам
            int totalPagesRead = 0;
            foreach (var book in books)
            {
                if (startDate.HasValue || endDate.HasValue)
                {
                    // Суммируем только страницы, прочитанные в указанный период
                    totalPagesRead += book.PagesReadHistory
                        .Where(p => (!startDate.HasValue || p.Date >= startDate.Value.Date) &&
                                   (!endDate.HasValue || p.Date <= endDate.Value.Date))
                        .Sum(p => p.PagesRead);
                }
                else
                {
                    // Суммируем все страницы
                    totalPagesRead += book.CurrentPage;
                }
            }
            
            return new LibraryStatistics
            {
                TotalBooks = filteredBooks.Count,
                ReadBooks = filteredBooks.Count(b => b.DateFinished.HasValue),
                CurrentBooks = filteredBooks.Count(b => b.IsCurrentlyReading),
                PlannedBooks = filteredBooks.Count(b => !b.IsCurrentlyReading && !b.DateFinished.HasValue),
                TotalPagesRead = totalPagesRead,
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
        /// Популярные авторы
        /// </summary>
        public List<AuthorStatistic> PopularAuthors { get; set; } = new List<AuthorStatistic>();
    }

    /// <summary>
    /// Статистика по автору
    /// </summary>
    public class AuthorStatistic
    {
        /// <summary>
        /// Имя автора
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Количество книг автора
        /// </summary>
        public int Count { get; set; }
    }
}
