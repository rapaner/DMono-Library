namespace Library.Models
{
    /// <summary>
    /// Статистика по автору
    /// </summary>
    public class AuthorStatistic
    {
        /// <summary>
        /// Имя автора
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Количество книг автора
        /// </summary>
        public int Count { get; set; }
    }
}

