namespace Library.Models
{
    /// <summary>
    /// Данные о чтении за один день
    /// </summary>
    public class DailyReadingData
    {
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Количество прочитанных страниц
        /// </summary>
        public int PagesRead { get; set; }
    }
}

