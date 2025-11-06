namespace Library.Models
{
    /// <summary>
    /// Ключи сервисов по выбору книг (идентификаторы и названия методов расчёта).
    /// </summary>
    public record BookChooseServiceKey
    {
        /// <summary>
        /// 0, "Приоритет первым"
        /// </summary>
        public static BookChooseServiceKey PrioritizedRandom = new(PrioritizedRandomId, "Приоритет первым");

        /// <summary>
        /// 1, "Наугад"
        /// </summary>
        public static BookChooseServiceKey Random = new(RandomId, "Наугад");

        protected BookChooseServiceKey(int id, string name)
        {
            Id = id;
            Name = name;
        }

        #region Константы

        public const int PrioritizedRandomId = 0;
        public const int RandomId = 1;

        #endregion Константы

        #region Свойства

        /// <summary>
        /// Код
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; protected set; }

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Получить список всех вариантов
        /// </summary>
        public static IEnumerable<BookChooseServiceKey> GetAll() => [PrioritizedRandom, Random];

        /// <summary>
        /// Получить элемент по коду
        /// </summary>
        /// <param name="id">Код</param>
        /// <exception cref="ArgumentException"></exception>
        public static BookChooseServiceKey Get(int id) => GetAll().FirstOrDefault(k => k.Id == id) ?? throw new ArgumentException($"Нет варианта с id {id}");

        #endregion Методы
    }
}