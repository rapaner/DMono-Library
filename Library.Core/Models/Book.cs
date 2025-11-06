using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models
{
    /// <summary>
    /// Модель книги для хранения в базе данных
    /// </summary>
    [Table("Books")]
    public partial record Book
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
        /// Коллекция записей о прочитанных страницах по датам
        /// </summary>
        public ICollection<PagesReadInDate> PagesReadHistory { get; set; } = new List<PagesReadInDate>();

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
        /// Расписание чтения книги (one-to-one связь)
        /// </summary>
        public BookReadingSchedule? ReadingSchedule { get; set; }

        /// <summary>
        /// Первая страница основного издания книги (для альтернативного расчета страниц)
        /// </summary>
        public int? MainFirstPage { get; set; }

        /// <summary>
        /// Первая страница альтернативного издания книги (для альтернативного расчета страниц)
        /// </summary>
        public int? AlternativeFirstPage { get; set; }

        /// <summary>
        /// Последняя страница альтернативного издания книги (для альтернативного расчета страниц)
        /// </summary>
        public int? AlternativeLastPage { get; set; }
    }
}