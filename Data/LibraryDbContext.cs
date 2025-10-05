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
        /// Коллекция авторов в базе данных
        /// </summary>
        public DbSet<Author> Authors { get; set; }

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

            // Настройка таблицы Authors
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Индекс для быстрого поиска по имени автора
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Authors_Name");
            });

            // Настройка таблицы Books
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SeriesTitle)
                    .HasMaxLength(200);

                entity.Property(e => e.DateAdded)
                    .IsRequired();

                // Настройка связи многие-ко-многим с авторами
                entity.HasMany(e => e.Authors)
                    .WithMany(e => e.Books)
                    .UsingEntity(j => j.ToTable("BookAuthors"));

                // Индексы для оптимизации запросов
                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("IX_Books_Title");

                entity.HasIndex(e => e.IsCurrentlyReading)
                    .HasDatabaseName("IX_Books_IsCurrentlyReading");

                entity.HasIndex(e => e.DateAdded)
                    .HasDatabaseName("IX_Books_DateAdded");

                entity.HasIndex(e => e.DateFinished)
                    .HasDatabaseName("IX_Books_DateFinished");

                entity.HasIndex(e => e.SeriesTitle)
                    .HasDatabaseName("IX_Books_SeriesTitle");

                // Значения по умолчанию
                entity.Property(e => e.CurrentPage)
                    .HasDefaultValue(0);

                entity.Property(e => e.IsCurrentlyReading)
                    .HasDefaultValue(false);
            });
        }
    }
}
