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
        /// Коллекция записей о прочитанных страницах
        /// </summary>
        public DbSet<PagesReadInDate> PagesReadHistory { get; set; }

        /// <summary>
        /// Коллекция расписаний чтения книг
        /// </summary>
        public DbSet<BookReadingSchedule> BookReadingSchedules { get; set; }

        /// <summary>
        /// Глобальные настройки часов чтения по умолчанию
        /// </summary>
        public DbSet<DefaultReadingHoursSettings> DefaultReadingHoursSettings { get; set; }

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

                // Настройка связи один-ко-многим с записями о прочитанных страницах
                entity.HasMany(e => e.PagesReadHistory)
                    .WithOne(e => e.Book)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

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
                entity.Property(e => e.IsCurrentlyReading)
                    .HasDefaultValue(false);

                // Настройка связи one-to-one с расписанием чтения
                entity.HasOne(e => e.ReadingSchedule)
                    .WithOne(e => e.Book)
                    .HasForeignKey<BookReadingSchedule>(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы BookReadingSchedules
            modelBuilder.Entity<BookReadingSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TargetFinishDate)
                    .IsRequired(); 
                
                entity.Property(e => e.StartHour);

                entity.Property(e => e.EndHour);

                // Индекс для быстрого поиска по BookId
                entity.HasIndex(e => e.BookId)
                    .HasDatabaseName("IX_BookReadingSchedules_BookId")
                    .IsUnique(); // One-to-one связь
            });

            // Настройка таблицы DefaultReadingHoursSettings
            modelBuilder.Entity<DefaultReadingHoursSettings>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DefaultStartHour)
                    .IsRequired()
                    .HasDefaultValue(6);

                entity.Property(e => e.DefaultEndHour)
                    .IsRequired()
                    .HasDefaultValue(23);

                // Добавляем начальные данные
                entity.HasData(new DefaultReadingHoursSettings
                {
                    Id = 1,
                    DefaultStartHour = 6,
                    DefaultEndHour = 23
                });
            });

            // Настройка таблицы PagesReadInDate
            modelBuilder.Entity<PagesReadInDate>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Date)
                    .IsRequired();

                entity.Property(e => e.PagesRead)
                    .IsRequired();

                // Индекс для быстрого поиска по книге и дате
                entity.HasIndex(e => new { e.BookId, e.Date })
                    .HasDatabaseName("IX_PagesReadInDate_BookId_Date")
                    .IsUnique(); // Только одна запись на книгу на дату
            });
        }
    }
}
