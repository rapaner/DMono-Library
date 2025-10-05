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
        /// Коллекция авторов книги
        /// </summary>
        public ICollection<Author> Authors { get; set; } = new List<Author>();

        /// <summary>
        /// Название цикла/серии книг (необязательное поле, максимум 200 символов)
        /// </summary>
        [MaxLength(200)]
        public string? SeriesTitle { get; set; }

        /// <summary>
        /// Номер книги в цикле/серии (необязательное поле)
        /// </summary>
        public int? SeriesNumber { get; set; }

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

        /// <summary>
        /// Текстовое представление авторов книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string AuthorsText => Authors != null && Authors.Any() 
            ? string.Join(", ", Authors.Select(a => a.Name)) 
            : "Автор не указан";
    }
}
