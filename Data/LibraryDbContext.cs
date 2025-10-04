using Microsoft.EntityFrameworkCore;
using Library.Models;

namespace Library.Data
{
    /// <summary>
    /// Контекст базы данных для библиотеки книг
    /// </summary>
    public class LibraryDbContext : DbContext
    {
        /// <summary>
        /// Коллекция книг в базе данных
        /// </summary>
        public DbSet<Book> Books { get; set; }

        /// <summary>
        /// Конструктор контекста базы данных
        /// </summary>
        /// <param name="options">Опции для настройки контекста</param>
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Настройка модели данных при создании контекста
        /// </summary>
        /// <param name="modelBuilder">Построитель модели</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Books
            modelBuilder.Entity<Book>(entity =>
            {
                // Настройка первичного ключа
                entity.HasKey(e => e.Id);
                
                // Настройка автоинкремента
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                // Настройка обязательных полей
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Genre)
                    .HasMaxLength(50);

                entity.Property(e => e.DateAdded)
                    .IsRequired();

                // Настройка индексов для оптимизации запросов
                entity.HasIndex(e => e.IsCurrentlyReading)
                    .HasDatabaseName("IX_Books_IsCurrentlyReading");

                entity.HasIndex(e => e.DateAdded)
                    .HasDatabaseName("IX_Books_DateAdded");

                entity.HasIndex(e => e.DateFinished)
                    .HasDatabaseName("IX_Books_DateFinished");

                // Настройка значений по умолчанию
                entity.Property(e => e.CurrentPage)
                    .HasDefaultValue(0);

                entity.Property(e => e.IsCurrentlyReading)
                    .HasDefaultValue(false);

                entity.Property(e => e.Rating)
                    .HasDefaultValue(0.0);

                entity.Property(e => e.Notes)
                    .HasDefaultValue(string.Empty);

                entity.Property(e => e.Genre)
                    .HasDefaultValue(string.Empty);
            });
        }
    }
}
