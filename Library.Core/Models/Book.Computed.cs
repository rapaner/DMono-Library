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
        /// Текущая страница, на которой остановился читатель (вычисляемое свойство)
        /// Рассчитывается как сумма всех прочитанных страниц из истории
        /// </summary>
        [NotMapped]
        public int CurrentPage => PagesReadHistory?.Sum(p => p.PagesRead) ?? 0;

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
        /// Дата начала чтения, вычисленная по первой записи в истории (null, если записи отсутствуют)
        /// </summary>
        [NotMapped]
        public DateTime? StartDateFromHistory =>
            PagesReadHistory != null && PagesReadHistory.Any()
                ? PagesReadHistory.Min(p => p.Date)
                : null;

    }
}