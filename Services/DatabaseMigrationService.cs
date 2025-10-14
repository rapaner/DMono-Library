using Microsoft.EntityFrameworkCore;
using Library.Core.Data;
using Library.Core.Models;
using Library.Models;
using Microsoft.Data.Sqlite;

namespace Library.Services
{
    /// <summary>
    /// Сервис для управления миграциями базы данных
    /// </summary>
    public class DatabaseMigrationService
    {
        private readonly LibraryDbContext _context;
        private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
        private readonly string _dbPath;

        /// <summary>
        /// Конструктор сервиса миграции
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="appConfig">Конфигурация приложения</param>
        public DatabaseMigrationService(LibraryDbContext context, IDbContextFactory<LibraryDbContext> contextFactory, AppConfiguration appConfig)
        {
            _context = context;
            _contextFactory = contextFactory;
            _dbPath = appConfig.DatabasePath;
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
        /// Создать таблицу истории миграций и добавить запись для InitialCreate
        /// </summary>
        public async Task CreateMigrationHistoryTableAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Creating migration history table ===");

                var connectionString = $"Data Source={_dbPath}";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Создаём таблицу __EFMigrationsHistory
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
                            MigrationId TEXT NOT NULL PRIMARY KEY,
                            ProductVersion TEXT NOT NULL
                        )";
                    await command.ExecuteNonQueryAsync();
                    System.Diagnostics.Debug.WriteLine("=== Migration history table created ===");
                }

                // Вставляем запись для InitialCreate миграции
                using (var command = connection.CreateCommand())
                {
                    // Получаем версию EF Core из контекста
                    var efCoreVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "9.0.0";
                    
                    command.CommandText = @"
                        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                        VALUES (@migrationId, @productVersion)";
                    
                    command.Parameters.AddWithValue("@migrationId", "20250101000000_InitialCreate");
                    command.Parameters.AddWithValue("@productVersion", efCoreVersion);
                    
                    await command.ExecuteNonQueryAsync();
                    System.Diagnostics.Debug.WriteLine($"=== InitialCreate migration record added (EF Core version: {efCoreVersion}) ===");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error creating migration history: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось создать таблицу истории миграций: {ex.Message}", ex);
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

                // Критично: закрываем текущее соединение и чистим пулы, чтобы снять блокировки
                await _context.Database.CloseConnectionAsync();
                SqliteConnection.ClearAllPools();

                // Используем «свежий» контекст из фабрики
                await using var ctx = await _contextFactory.CreateDbContextAsync();

                // Улучшаем совместимость с параллельными чтениями
                try
                {
                    await ctx.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
                    await ctx.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;");
                }
                catch { /* PRAGMA может быть недоступен в некоторых средах */ }

                // Защита от зависшего миграционного лока (__EFMigrationsLock)
                try
                {
                    // Удаляем блокировку, если она старше 2 минут
                    await ctx.Database.ExecuteSqlRawAsync(
                        "DELETE FROM \"__EFMigrationsLock\" WHERE \"Id\"=1 AND (julianday('now') - julianday(\"Timestamp\")) > (2.0/1440.0);");
                }
                catch
                {
                    // Игнорируем: таблицы может не быть до первой миграции
                }

                // Диагностика: список ожидающих и применённых
                var pendingMigrations = await ctx.Database.GetPendingMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Pending migrations count: {pendingMigrations.Count()} ===");
                foreach (var m in pendingMigrations)
                {
                    System.Diagnostics.Debug.WriteLine($"    - {m}");
                }

                if (!pendingMigrations.Any())
                {
                    System.Diagnostics.Debug.WriteLine("=== No pending migrations ===");
                    return;
                }

                var appliedMigrations = await ctx.Database.GetAppliedMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Applied migrations count: {appliedMigrations.Count()} ===");
                foreach (var m in appliedMigrations)
                {
                    System.Diagnostics.Debug.WriteLine($"    - {m}");
                }

                // Применяем миграции с увеличенным таймаутом
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                await ctx.Database.MigrateAsync(cts.Token);

                System.Diagnostics.Debug.WriteLine("=== Migrations applied successfully ===");

                var finalMigrations = await ctx.Database.GetAppliedMigrationsAsync();
                System.Diagnostics.Debug.WriteLine($"=== Final applied migrations count: {finalMigrations.Count()} ===");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("=== Migration timeout ===");
                throw new InvalidOperationException("Миграция превысила таймаут");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error applying migrations: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                throw new InvalidOperationException($"Не удалось применить миграции: {ex.Message}", ex);
            }
        }

    }
}

