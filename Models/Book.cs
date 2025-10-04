using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    /// <summary>
    /// Модель книги для хранения в базе данных
    /// </summary>
    [Table("Books")]
    public record Book
    {
        /// <summary>
        /// Уникальный идентификатор книги в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Название книги (максимум 200 символов)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Автор книги (максимум 100 символов)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Жанр книги (максимум 50 символов)
        /// </summary>
        [MaxLength(50)]
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Общее количество страниц в книге
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Количество страниц должно быть больше 0")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Текущая страница, на которой остановился читатель
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Текущая страница не может быть отрицательной")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// Флаг, указывающий читается ли книга в данный момент
        /// </summary>
        public bool IsCurrentlyReading { get; set; }

        /// <summary>
        /// Дата добавления книги в библиотеку
        /// </summary>
        [Required]
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Дата завершения чтения книги (null если книга не прочитана)
        /// </summary>
        public DateTime? DateFinished { get; set; }

        /// <summary>
        /// Рейтинг книги от 0 до 5 звезд
        /// </summary>
        [Range(0, 5, ErrorMessage = "Рейтинг должен быть от 0 до 5")]
        public double Rating { get; set; }

        /// <summary>
        /// Личные заметки читателя о книге
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Процент прочитанных страниц (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public double ProgressPercentage => TotalPages > 0 ? (double)CurrentPage / TotalPages * 100 : 0;

        /// <summary>
        /// Текстовое представление прогресса чтения (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string ProgressText => $"{CurrentPage} / {TotalPages} страниц";

        /// <summary>
        /// Текстовое представление статуса книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string StatusText => IsCurrentlyReading ? "Читаю сейчас" : 
                                   DateFinished.HasValue ? "Прочитано" : "В планах";
    }
}
