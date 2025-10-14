using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models
{
    /// <summary>
    /// Модель записи о количестве прочитанных страниц за день
    /// </summary>
    [Table("PagesReadInDate")]
    public record PagesReadInDate
    {
        /// <summary>
        /// Уникальный идентификатор записи в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор книги, к которой относится запись
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Книга, к которой относится запись
        /// </summary>
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Дата, за которую записано количество прочитанных страниц
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Количество страниц, прочитанных за этот день
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Количество страниц должно быть больше 0")]
        public int PagesRead { get; set; }
    }
}