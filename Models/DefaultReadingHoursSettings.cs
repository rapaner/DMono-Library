using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    /// <summary>
    /// Модель для хранения глобальных настроек часов чтения по умолчанию
    /// </summary>
    [Table("DefaultReadingHoursSettings")]
    public record DefaultReadingHoursSettings
    {
        /// <summary>
        /// Уникальный идентификатор (всегда 1, так как это единственная запись настроек)
        /// </summary>
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>
        /// Час начала чтения по умолчанию
        /// </summary>
        [Required]
        [Range(0, 23)]
        public int DefaultStartHour { get; set; } = 6;

        /// <summary>
        /// Час окончания чтения по умолчанию
        /// </summary>
        [Required]
        [Range(1, 24)]
        public int DefaultEndHour { get; set; } = 23;
    }
}

