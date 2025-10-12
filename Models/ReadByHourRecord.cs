namespace Library.Models
{
    /// <summary>
    /// Запись о чтении в час
    /// </summary>
    public record ReadByHourRecord
    {
        public ReadByHourRecord(DateTime date, decimal pages)
        {
            DateInternal = date;
            PagesInternal = pages;
        }

        /// <summary>
        /// Отметка
        /// </summary>
        protected DateTime DateInternal { get; }

        /// <summary>
        /// Количество страниц
        /// </summary>
        protected decimal PagesInternal { get; }

        public string TimeStamp => $"{DateInternal.ToShortDateString()} {DateInternal.ToShortTimeString()}";

        /// <summary>
        /// Количество для отображения
        /// </summary>
        public decimal Pages => Math.Ceiling(PagesInternal);
    }
}

