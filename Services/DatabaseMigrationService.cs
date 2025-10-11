using Microsoft.EntityFrameworkCore;
using Library.Data;
using System.Data;
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

        /// <summary>
        /// Конструктор сервиса миграции
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="dbPath">Путь к файлу базы данных</param>
        public DatabaseMigrationService(LibraryDbContext context, string dbPath)
        {
            _context = context;
            _dbPath = dbPath;
            _backupPath = $"{dbPath}.backup-{DateTime.Now:yyyyMMddHHmmss}";
        }

        /// <summary>
        /// Проверить существование таблицы истории миграций
        /// </summary>
        /// <returns>True, если таблица существует</returns>
        public async Task<bool> IsMigrationHistoryTableExistsAsync()
        {
            try
            {
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
        /// Создать резервную копию базы данных
        /// </summary>
        public async Task CreateBackupAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Creating backup: {_backupPath} ===");
                
                // Копируем файл базы данных
                await Task.Run(() => File.Copy(_dbPath, _backupPath, overwrite: true));
                
                System.Diagnostics.Debug.WriteLine("=== Backup created successfully ===");
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
                
                await _context.Database.MigrateAsync();
                
                System.Diagnostics.Debug.WriteLine("=== Migrations applied successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error applying migrations: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось применить миграции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Восстановить данные из резервной копии после применения миграций
        /// </summary>
        public async Task RestoreDataFromBackupAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Restoring data from backup ===");

                if (!File.Exists(_backupPath))
                {
                    System.Diagnostics.Debug.WriteLine("=== Backup file not found, skipping restore ===");
                    return;
                }

                var connectionString = $"Data Source={_dbPath}";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Подключаем бэкап базы данных
                using (var attachCommand = connection.CreateCommand())
                {
                    attachCommand.CommandText = $"ATTACH DATABASE '{_backupPath}' AS backup";
                    await attachCommand.ExecuteNonQueryAsync();
                    System.Diagnostics.Debug.WriteLine("=== Backup database attached ===");
                }

                // Копируем данные из таблиц в правильном порядке (соблюдая внешние ключи)
                
                // 1. Копируем Authors
                await CopyTableData(connection, "Authors");

                // 2. Копируем Books
                await CopyTableData(connection, "Books");

                // 3. Копируем BookAuthors (связь многие-ко-многим)
                await CopyTableData(connection, "BookAuthors");

                // 4. Копируем PagesReadInDate
                await CopyTableData(connection, "PagesReadInDate");

                // Отключаем бэкап базы данных
                using (var detachCommand = connection.CreateCommand())
                {
                    detachCommand.CommandText = "DETACH DATABASE backup";
                    await detachCommand.ExecuteNonQueryAsync();
                    System.Diagnostics.Debug.WriteLine("=== Backup database detached ===");
                }

                System.Diagnostics.Debug.WriteLine("=== Data restored successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error restoring data: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось восстановить данные из резервной копии: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Копировать данные из таблицы бэкапа в основную базу
        /// </summary>
        private async Task CopyTableData(SqliteConnection connection, string tableName)
        {
            try
            {
                // Проверяем, есть ли таблица в бэкапе
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = $@"
                    SELECT COUNT(*) 
                    FROM backup.sqlite_master 
                    WHERE type='table' 
                    AND name='{tableName}'";
                
                var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (!tableExists)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Table {tableName} not found in backup, skipping ===");
                    return;
                }

                // Проверяем, есть ли данные в таблице
                using var countCommand = connection.CreateCommand();
                countCommand.CommandText = $"SELECT COUNT(*) FROM backup.{tableName}";
                var rowCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                if (rowCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Table {tableName} is empty in backup, skipping ===");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"=== Copying {rowCount} rows from {tableName} ===");

                // Копируем данные
                using var copyCommand = connection.CreateCommand();
                copyCommand.CommandText = $"INSERT INTO main.{tableName} SELECT * FROM backup.{tableName}";
                var copiedRows = await copyCommand.ExecuteNonQueryAsync();

                System.Diagnostics.Debug.WriteLine($"=== Copied {copiedRows} rows to {tableName} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error copying table {tableName}: {ex.Message} ===");
                throw new InvalidOperationException($"Не удалось скопировать данные таблицы {tableName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Удалить файл резервной копии
        /// </summary>
        public void DeleteBackup()
        {
            try
            {
                if (File.Exists(_backupPath))
                {
                    File.Delete(_backupPath);
                    System.Diagnostics.Debug.WriteLine($"=== Backup deleted: {_backupPath} ===");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error deleting backup: {ex.Message} ===");
                // Не выбрасываем исключение, так как это не критично
            }
        }
    }
}

