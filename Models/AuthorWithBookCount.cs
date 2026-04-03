namespace Library.Models
{
    /// <summary>
    /// Модель для отображения автора с количеством книг
    /// </summary>
    public class AuthorWithBookCount
    {
        /// <summary>
        /// Идентификатор автора
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя автора
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Количество книг автора
        /// </summary>
        public int BookCount { get; set; }
    }
}
