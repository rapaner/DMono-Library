namespace Library.Models
{
    /// <summary>
    /// Модель для отображения рейтинга книг по среднему количеству прочитанных страниц в день
    /// </summary>
    public class BookRanking
    {
        /// <summary>
        /// Идентификатор книги
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Название книги
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Авторы книги (текстовое представление)
        /// </summary>
        public string AuthorsText { get; set; } = string.Empty;

        /// <summary>
        /// Всего страниц прочитано
        /// </summary>
        public int TotalPagesRead { get; set; }

        /// <summary>
        /// Количество уникальных дней с записями о чтении
        /// </summary>
        public int UniqueDaysCount { get; set; }

        /// <summary>
        /// Среднее количество страниц в день
        /// </summary>
        public double AveragePagesPerDay { get; set; }
    }
}

