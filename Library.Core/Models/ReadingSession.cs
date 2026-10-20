using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models
{
    /// <summary>
    /// Одно прочтение книги (включая перечитывания)
    /// </summary>
    [Table("ReadingSessions")]
    public class ReadingSession
    {
        /// <summary>
        /// Уникальный идентификатор прочтения
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор книги
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Книга
        /// </summary>
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Порядковый номер прочтения для книги (1, 2, 3…)
        /// </summary>
        [Required]
        public int Number { get; set; }

        /// <summary>
        /// Дата начала прочтения
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата завершения прочтения (null, если чтение ещё не завершено)
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Записи о прочитанных страницах в рамках этого прочтения
        /// </summary>
        public ICollection<PagesReadInDate> PagesReadHistory { get; set; } = new List<PagesReadInDate>();
    }
}
