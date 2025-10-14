namespace Library.Models
{
    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Путь к файлу базы данных
        /// </summary>
        public string DatabasePath { get; set; } = string.Empty;

        /// <summary>
        /// Имя файла базы данных
        /// </summary>
        public string DatabaseFileName { get; set; } = "library.db";

        /// <summary>
        /// Директория для хранения данных приложения
        /// </summary>
        public string AppDataDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Версия приложения
        /// </summary>
        public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Имя приложения
    /// </summary>
    public string AppName { get; set; } = "Library";

    /// <summary>
    /// Час начала чтения по умолчанию
    /// </summary>
    public int DefaultStartHour { get; set; } = 6;

    /// <summary>
    /// Час окончания чтения по умолчанию
    /// </summary>
    public int DefaultEndHour { get; set; } = 23;
}
}

