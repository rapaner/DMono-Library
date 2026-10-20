namespace Library.Models
{
    /// <summary>
    /// Элемент списка прочтений на странице книги
    /// </summary>
    public class ReadingSessionDisplayItem
    {
        public int SessionId { get; set; }

        public int Number { get; set; }

        public string Title { get; set; } = string.Empty;

        public string DateRangeText { get; set; } = string.Empty;

        public string StatsText { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
