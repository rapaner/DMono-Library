using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Library.Services
{
    /// <summary>
    /// Сервис для управления миграциями базы данных
    /// </summary>
    public class DatabaseMigrationService
    {
        private readonly LibraryDbContext _context;
        private readonly string _dbPath;
        private readonly string _backupPath;
        private readonly string _jsonBackupPath;

        /// <summary>
        /// Конструктор сервиса миграции
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="dbPath">Путь к файлу базы данных</param>
        public DatabaseMigrationService(LibraryDbContext context, string dbPath)
        {
            _context = context;
            _dbPath = dbPath;
            _backupPath = $"{_dbPath}.backup-{DateTime.Now:yyyyMMddHHmmss}";
            _jsonBackupPath = $"{_dbPath}.backup-{DateTime.Now:yyyyMMddHHmmss}.json";
        }
    
    /// <summary>
    /// Модель для сериализации данных БД в JSON
    /// </summary>
    private class DatabaseBackup
    {
        public List<AuthorDto> Authors { get; set; } = new();
        public List<BookDto> Books { get; set; } = new();
        public List<BookAuthorDto> BookAuthors { get; set; } = new();
        public List<PagesReadDto> PagesReadHistory { get; set; } = new();
    }

    private class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? SeriesTitle { get; set; }
        public int? SeriesNumber { get; set; }
        public int TotalPages { get; set; }
        public bool IsCurrentlyReading { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateFinished { get; set; }
    }

    private class BookAuthorDto
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }
    }

    private class PagesReadDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public DateTime Date { get; set; }
        public int PagesRead { get; set; }
    }

        /// <summary>
        /// Проверить доступные миграции в сборке
        /// </summary>
        public void CheckAvailableMigrations()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Checking available migrations in assembly ===");
                
                var assembly = typeof(DatabaseMigrationService).Assembly;
                System.Diagnostics.Debug.WriteLine($"=== Assembly: {assembly.GetName().Name} ===");
                System.Diagnostics.Debug.WriteLine($"=== Assembly location: {System.AppContext.BaseDirectory} ===");
                
                // Показываем все типы в сборке, связанные с миграциями
                var allTypes = assembly.GetTypes()
                    .Where(t => t.Namespace != null && t.Namespace.Contains("Migration"))
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"=== Found {allTypes.Count} types in migration namespaces ===");
                foreach (var type in allTypes)
                {
                    System.Diagnostics.Debug.WriteLine($"    Type: {type.FullName}, BaseType: {type.BaseType?.FullName}");
                }
                
                // Ищем все классы, наследующие Migration
                var migrationTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null)
                    .Where(t => t.BaseType!.Name == "Migration" || 
                               t.BaseType.FullName?.Contains("Microsoft.EntityFrameworkCore.Migrations.Migration") == true)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"=== Found {migrationTypes.Count} migration classes ===");
                
                foreach (var migrationType in migrationTypes)
                {
                    System.Diagnostics.Debug.WriteLine($"    - {migrationType.FullName} (BaseType: {migrationType.BaseType?.FullName})");
                }
                
                // Проверяем через EF Core API
                System.Diagnostics.Debug.WriteLine("=== Checking via EF Core Migrations API ===");
                try
                {
                    var migrations = _context.Database.GetMigrations();
                    System.Diagnostics.Debug.WriteLine($"=== EF Core found {migrations.Count()} migrations ===");
                    foreach (var migration in migrations)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - {migration}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Error getting migrations via EF Core: {ex.Message} ===");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error checking migrations: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            }
        }

        /// <summary>
        /// Проверить существование таблицы истории миграций
        /// </summary>
        /// <returns>True, если таблица существует</returns>
        public async Task<bool> IsMigrationHistoryTableExistsAsync()
        {
            try
            {
                // Проверяем доступные миграции в сборке
                CheckAvailableMigrations();
                
                // Проверяем существование файла БД
                if (!File.Exists(_dbPath))
                {
                    System.Diagnostics.Debug.WriteLine("=== Database file does not exist ===");
                    return false;
                }

                // Проверяем существование таблицы __EFMigrationsHistory
                var connectionString = $"Data Source={_dbPath}";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM sqlite_master 
                    WHERE type='table' 
                    AND name='__EFMigrationsHistory'";

                var result = await command.ExecuteScalarAsync();
                var exists = Convert.ToInt32(result) > 0;

                System.Diagnostics.Debug.WriteLine($"=== Migration history table exists: {exists} ===");
                return exists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error checking migration history: {ex.Message} ===");
                return false;
            }
        }

        /// <summary>
        /// Экспортировать данные из БД в JSON
        /// </summary>
        public async Task ExportDataToJsonAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Exporting data to JSON ===");

                var backup = new DatabaseBackup();

                // Создаём новый контекст для чтения старой БД
                var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
                
                using var oldContext = new LibraryDbContext(optionsBuilder.Options);

                // Проверяем, что БД существует и содержит таблицы
                if (!await oldContext.Database.CanConnectAsync())
                {
                    System.Diagnostics.Debug.WriteLine("=== Cannot connect to database, skipping export ===");
                    return;
                }

                // Экспортируем авторов
                try
                {
                    var authors = await oldContext.Authors.AsNoTracking().ToListAsync();
                    backup.Authors = authors.Select(a => new AuthorDto
                    {
                        Id = a.Id,
                        Name = a.Name
                    }).ToList();
                    System.Diagnostics.Debug.WriteLine($"=== Exported {backup.Authors.Count} authors ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Error exporting authors: {ex.Message} ===");
                }

                // Экспортируем книги
                try
                {
                    var books = await oldContext.Books.AsNoTracking().ToListAsync();
                    backup.Books = books.Select(b => new BookDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        SeriesTitle = b.SeriesTitle,
                        SeriesNumber = b.SeriesNumber,
                        TotalPages = b.TotalPages,
                        IsCurrentlyReading = b.IsCurrentlyReading,
                        DateAdded = b.DateAdded,
                        DateFinished = b.DateFinished
                    }).ToList();
                    System.Diagnostics.Debug.WriteLine($"=== Exported {backup.Books.Count} books ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Error exporting books: {ex.Message} ===");
                }

                // Экспортируем связи книг и авторов через прямой SQL запрос
                try
                {
                    var connection = oldContext.Database.GetDbConnection();
                    await connection.OpenAsync();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT AuthorsId, BooksId FROM BookAuthors";
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        backup.BookAuthors.Add(new BookAuthorDto
                        {
                            AuthorId = reader.GetInt32(0),
                            BookId = reader.GetInt32(1)
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"=== Exported {backup.BookAuthors.Count} book-author relations ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Error exporting book-author relations: {ex.Message} ===");
                }

                // Экспортируем историю чтения
                try
                {
                    var pagesRead = await oldContext.PagesReadHistory.AsNoTracking().ToListAsync();
                    backup.PagesReadHistory = pagesRead.Select(p => new PagesReadDto
                    {
                        Id = p.Id,
                        BookId = p.BookId,
                        Date = p.Date,
                        PagesRead = p.PagesRead
                    }).ToList();
                    System.Diagnostics.Debug.WriteLine($"=== Exported {backup.PagesReadHistory.Count} reading history records ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Error exporting reading history: {ex.Message} ===");
                }

                // Сохраняем в JSON файл
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(backup, options);
                await File.WriteAllTextAsync(_jsonBackupPath, json);

                System.Diagnostics.Debug.WriteLine($"=== Data exported to: {_jsonBackupPath} ===");
                System.Diagnostics.Debug.WriteLine($"=== JSON file size: {new FileInfo(_jsonBackupPath).Length} bytes ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error exporting data to JSON: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось экспортировать данные в JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создать резервную копию базы данных (файл БД)
        /// </summary>
        public async Task CreateBackupAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Creating database file backup: {_backupPath} ===");
                
                // Копируем файл базы данных
                await Task.Run(() => File.Copy(_dbPath, _backupPath, overwrite: true));
                
                System.Diagnostics.Debug.WriteLine("=== Database file backup created successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error creating backup: {ex.Message} ===");
                throw new InvalidOperationException($"Не удалось создать резервную копию базы данных: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Применить все ожидающие миграции
        /// </summary>
        public async Task MigrateAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Applying migrations ===");

                // Получаем список ожидающих миграций
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Pending migrations count: {pendingMigrations.Count()} ===");
                
                foreach (var migration in pendingMigrations)
                {
                    System.Diagnostics.Debug.WriteLine($"    - {migration}");
                }

                // Получаем список уже применённых миграций
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Applied migrations count: {appliedMigrations.Count()} ===");
                
                foreach (var migration in appliedMigrations)
                {
                    System.Diagnostics.Debug.WriteLine($"    - {migration}");
                }

                // Применяем миграции
                await _context.Database.MigrateAsync();
                
                System.Diagnostics.Debug.WriteLine("=== Migrations applied successfully ===");
                
                // Проверяем финальное состояние
                var finalMigrations = await _context.Database.GetAppliedMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Final applied migrations count: {finalMigrations.Count()} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error applying migrations: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось применить миграции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Импортировать данные из JSON после применения миграций
        /// </summary>
        public async Task ImportDataFromJsonAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Importing data from JSON ===");

                if (!File.Exists(_jsonBackupPath))
                {
                    System.Diagnostics.Debug.WriteLine("=== JSON backup file not found, skipping import ===");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"=== JSON backup file exists: {_jsonBackupPath} ===");
                System.Diagnostics.Debug.WriteLine($"=== JSON file size: {new FileInfo(_jsonBackupPath).Length} bytes ===");

                // Читаем JSON файл
                var json = await File.ReadAllTextAsync(_jsonBackupPath);
                var backup = JsonSerializer.Deserialize<DatabaseBackup>(json);

                if (backup == null)
                {
                    System.Diagnostics.Debug.WriteLine("=== Failed to deserialize JSON backup ===");
                    return;
                }

                // Отключаем проверку внешних ключей для ускорения импорта
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");

                // Импортируем авторов
                if (backup.Authors.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"=== Importing {backup.Authors.Count} authors ===");
                    foreach (var authorDto in backup.Authors)
                    {
                        var author = new Author
                        {
                            Id = authorDto.Id,
                            Name = authorDto.Name
                        };
                        _context.Authors.Add(author);
                    }
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("=== Authors imported ===");
                }

                // Импортируем книги
                if (backup.Books.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"=== Importing {backup.Books.Count} books ===");
                    foreach (var bookDto in backup.Books)
                    {
                        var book = new Book
                        {
                            Id = bookDto.Id,
                            Title = bookDto.Title,
                            SeriesTitle = bookDto.SeriesTitle,
                            SeriesNumber = bookDto.SeriesNumber,
                            TotalPages = bookDto.TotalPages,
                            IsCurrentlyReading = bookDto.IsCurrentlyReading,
                            DateAdded = bookDto.DateAdded,
                            DateFinished = bookDto.DateFinished
                        };
                        _context.Books.Add(book);
                    }
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("=== Books imported ===");
                }

                // Импортируем связи книг и авторов
                if (backup.BookAuthors.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"=== Importing {backup.BookAuthors.Count} book-author relations ===");
                    
                    // Используем прямой SQL для вставки в таблицу связей
                    foreach (var relation in backup.BookAuthors)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO BookAuthors (AuthorsId, BooksId) VALUES ({0}, {1})",
                            relation.AuthorId, relation.BookId);
                    }
                    System.Diagnostics.Debug.WriteLine("=== Book-author relations imported ===");
                }

                // Импортируем историю чтения
                if (backup.PagesReadHistory.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"=== Importing {backup.PagesReadHistory.Count} reading history records ===");
                    foreach (var pagesDto in backup.PagesReadHistory)
                    {
                        var pagesRead = new PagesReadInDate
                        {
                            Id = pagesDto.Id,
                            BookId = pagesDto.BookId,
                            Date = pagesDto.Date,
                            PagesRead = pagesDto.PagesRead
                        };
                        _context.PagesReadHistory.Add(pagesRead);
                    }
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("=== Reading history imported ===");
                }

                // Включаем обратно проверку внешних ключей
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");

                System.Diagnostics.Debug.WriteLine("=== Data imported successfully from JSON ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error importing data from JSON: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                
                // Включаем обратно проверку внешних ключей в случае ошибки
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");
                }
                catch { }
                
                throw new InvalidOperationException($"Не удалось импортировать данные из JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Удалить файлы резервных копий (БД и JSON)
        /// </summary>
        public void DeleteBackups()
        {
            try
            {
                // Удаляем JSON бэкап
                if (File.Exists(_jsonBackupPath))
                {
                    File.Delete(_jsonBackupPath);
                    System.Diagnostics.Debug.WriteLine($"=== JSON backup deleted: {_jsonBackupPath} ===");
                }

                // Удаляем файловый бэкап БД
                if (File.Exists(_backupPath))
                {
                    File.Delete(_backupPath);
                    System.Diagnostics.Debug.WriteLine($"=== Database backup deleted: {_backupPath} ===");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error deleting backups: {ex.Message} ===");
                // Не выбрасываем исключение, так как это не критично
            }
        }
    }
}

