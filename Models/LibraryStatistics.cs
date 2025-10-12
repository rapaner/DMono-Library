namespace Library.Models
{
    /// <summary>
    /// Статистика библиотеки
    /// </summary>
    public class LibraryStatistics
    {
        /// <summary>
        /// Общее количество книг
        /// </summary>
        public int TotalBooks { get; set; }

        /// <summary>
        /// Количество прочитанных книг
        /// </summary>
        public int ReadBooks { get; set; }

        /// <summary>
        /// Количество книг, читаемых сейчас
        /// </summary>
        public int CurrentBooks { get; set; }

        /// <summary>
        /// Количество книг в планах
        /// </summary>
        public int PlannedBooks { get; set; }

        /// <summary>
        /// Общее количество прочитанных страниц
        /// </summary>
        public int TotalPagesRead { get; set; }

        /// <summary>
        /// Популярные авторы
        /// </summary>
        public List<AuthorStatistic> PopularAuthors { get; set; } = new List<AuthorStatistic>();
    }
}

