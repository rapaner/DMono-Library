using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Library.Core.Models
{
    /// <summary>
    /// Вычисляемые свойства модели Book
    /// </summary>
    public partial class Book
    {
        /// <summary>
        /// Текущее прочтение: открытое (без даты завершения) или последнее по номеру
        /// </summary>
        [NotMapped]
        public ReadingSession? CurrentSession
        {
            get
            {
                if (ReadingSessions == null || !ReadingSessions.Any())
                    return null;

                var open = ReadingSessions.FirstOrDefault(s => s.FinishDate == null);
                return open ?? ReadingSessions.OrderByDescending(s => s.Number).First();
            }
        }

        /// <summary>
        /// Записи истории текущего прочтения
        /// </summary>
        [NotMapped]
        private IEnumerable<PagesReadInDate> CurrentSessionHistory =>
            CurrentSession?.PagesReadHistory ?? Enumerable.Empty<PagesReadInDate>();

        /// <summary>
        /// Текущая страница в рамках текущего прочтения
        /// </summary>
        [NotMapped]
        public int CurrentPage => CurrentSessionHistory.Sum(p => p.PagesRead);

        /// <summary>
        /// Процент прочитанных страниц (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public double ProgressPercentage => TotalPages > 0 ? (double)CurrentPage / TotalPages * 100 : 0;

        /// <summary>
        /// Текстовое представление прогресса чтения (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string ProgressText => $"{CurrentPage} / {TotalPages} страниц";

        /// <summary>
        /// Книга читается в данный момент (для UI и фильтров, не хранится в БД)
        /// </summary>
        [NotMapped]
        public bool IsCurrentlyReading => Status == BookStatus.Reading;

        /// <summary>
        /// Текстовое представление статуса книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string StatusText => Status switch
        {
            BookStatus.Reading => "Читаю сейчас",
            BookStatus.Finished => "Прочитано",
            BookStatus.FinishedLongAgo => "Прочитана давно",
            BookStatus.Planned => "В планах",
            BookStatus.WontRead => "Не буду читать",
            _ => "Неизвестно"
        };

        /// <summary>
        /// Текстовое представление авторов книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string AuthorsText => Authors != null && Authors.Any()
            ? string.Join(", ", Authors.Select(a => a.Name))
            : "Автор не указан";

        /// <summary>
        /// Дата начала текущего прочтения по первой записи в его истории
        /// </summary>
        [NotMapped]
        public DateTime? StartDateFromHistory
        {
            get
            {
                var history = CurrentSessionHistory.ToList();
                if (history.Count == 0)
                    return CurrentSession?.StartDate;

                return history.Min(p => p.Date);
            }
        }

        /// <summary>
        /// Число завершённых прочтений
        /// </summary>
        [NotMapped]
        public int ReadCount => ReadingSessions?.Count(s => s.FinishDate != null) ?? 0;
    }
}
