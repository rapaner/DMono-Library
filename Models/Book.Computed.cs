using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    /// <summary>
    /// Вычисляемые свойства модели Book
    /// </summary>
    public partial record Book
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
        /// Статус чтения книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public BookStatus Status
        {
            get
            {
                if (IsCurrentlyReading)
                    return BookStatus.Reading;
                if (DateFinished.HasValue)
                    return BookStatus.Finished;
                return BookStatus.Planned;
            }
        }

        /// <summary>
        /// Текстовое представление статуса книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string StatusText => Status switch
        {
            BookStatus.Reading => "Читаю сейчас",
            BookStatus.Finished => "Прочитано",
            BookStatus.Planned => "В планах",
            _ => "Неизвестно"
        };

        /// <summary>
        /// Текстовое представление авторов книги (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public string AuthorsText => Authors != null && Authors.Any() 
            ? string.Join(", ", Authors.Select(a => a.Name)) 
            : "Автор не указан";
    }
}

