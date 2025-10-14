using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Core.Models
{
    /// <summary>
    /// Модель для хранения параметров расписания чтения книги
    /// </summary>
    [Table("BookReadingSchedules")]
    public record BookReadingSchedule
    {
        /// <summary>
        /// Уникальный идентификатор расписания
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ на книгу
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге
        /// </summary>
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Целевая дата окончания чтения
        /// </summary>
        [Required]
        public DateTime TargetFinishDate { get; set; }

        /// <summary>
        /// Час начала чтения (nullable - если не задан, используется глобальное значение по умолчанию)
        /// </summary>
        [Range(0, 23)]
        public int? StartHour { get; set; }

        /// <summary>
        /// Час окончания чтения (nullable - если не задан, используется глобальное значение по умолчанию)
        /// </summary>
        [Range(1, 24)]
        public int? EndHour { get; set; }
    }
}