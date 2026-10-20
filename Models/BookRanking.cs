namespace Library.Models
{
    /// <summary>
    /// Модель для отображения рейтинга книг по среднему количеству прочитанных страниц в день
    /// </summary>
    public class BookRanking
    {
        /// <summary>
        /// Порядковый номер в рейтинге
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Идентификатор книги
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Идентификатор прочтения
        /// </summary>
        public int ReadingSessionId { get; set; }

        /// <summary>
        /// Номер прочтения для книги
        /// </summary>
        public int ReadingNumber { get; set; }

        /// <summary>
        /// Метка перечитывания (например, «×2»)
        /// </summary>
        public string ReadingLabel { get; set; } = string.Empty;

        /// <summary>
        /// Название книги
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Заголовок с меткой перечитывания для отображения
        /// </summary>
        public string TitleWithReadingLabel =>
            string.IsNullOrEmpty(ReadingLabel) ? Title : $"{Title} {ReadingLabel}";

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

