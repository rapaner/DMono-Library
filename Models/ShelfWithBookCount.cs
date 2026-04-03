namespace Library.Models
{
    /// <summary>
    /// Модель для отображения полки с количеством книг
    /// </summary>
    public class ShelfWithBookCount
    {
        /// <summary>
        /// Идентификатор полки
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название полки
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Количество книг на полке
        /// </summary>
        public int BookCount { get; set; }
    }
}
