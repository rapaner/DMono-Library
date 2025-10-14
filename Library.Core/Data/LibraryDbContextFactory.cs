using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library.Core.Data
{
    /// <summary>
    /// Фабрика для создания контекста базы данных во время разработки
    /// </summary>
    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        /// <summary>
        /// Создать контекст базы данных для миграций
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Контекст базы данных</returns>
        public LibraryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();

            // Используем временную базу данных для миграций
            // Явно указываем сборку с миграциями
            optionsBuilder.UseSqlite(
                "Data Source=library.db",
                sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(LibraryDbContext).Assembly.GetName().Name));

            return new LibraryDbContext(optionsBuilder.Options);
        }
    }
}