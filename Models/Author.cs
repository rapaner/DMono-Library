using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    /// <summary>
    /// Модель автора для хранения в базе данных
    /// </summary>
    [Table("Authors")]
    public record Author
    {
        /// <summary>
        /// Уникальный идентификатор автора в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Имя автора (максимум 100 символов)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Коллекция книг этого автора
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
