namespace Library.Services
{
    /// <summary>
    /// Сервис выбора книги. Возвращает один номер книги на основе заданного количества.
    /// </summary>
    public interface IBookChooseService
    {
        /// <summary>
        /// Выбрать один номер книги из диапазона [1..booksAmount].
        /// </summary>
        /// <param name="booksAmount">Общее количество книг (должно быть > 0)</param>
        /// <returns>Номер выбранной книги</returns>
        Task<int> ChooseBook(int booksAmount);
    }
}