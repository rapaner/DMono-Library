using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models
{
    /// <summary>
    /// Модель полки для хранения в базе данных
    /// </summary>
    [Table("Shelves")]
    public class Shelf
    {
        /// <summary>
        /// Уникальный идентификатор полки в базе данных
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Название полки (максимум 100 символов)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Коллекция книг на этой полке
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
