namespace Library.Core.Models
{
    /// <summary>
    /// Статус чтения книги
    /// </summary>
    public enum BookStatus
    {
        /// <summary>
        /// Книга в планах на чтение
        /// </summary>
        Planned = 0,

        /// <summary>
        /// Книга читается в данный момент
        /// </summary>
        Reading = 1,

        /// <summary>
        /// Книга прочитана
        /// </summary>
        Finished = 2
    }
}