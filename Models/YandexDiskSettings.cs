namespace Library.Models
{
    /// <summary>
    /// Настройки для работы с Яндекс Диском
    /// </summary>
    public class YandexDiskSettings
    {
        /// <summary>
        /// OAuth токен для доступа к Яндекс Диску
        /// </summary>
        public string? OAuthToken { get; set; }

        /// <summary>
        /// Автоматическое резервное копирование включено
        /// </summary>
        public bool AutoBackupEnabled { get; set; }

        /// <summary>
        /// Частота автоматического резервного копирования (в днях)
        /// </summary>
        public int AutoBackupFrequencyDays { get; set; } = 7;

        /// <summary>
        /// Дата последнего резервного копирования
        /// </summary>
        public DateTime? LastBackupDate { get; set; }

        /// <summary>
        /// Максимальное количество резервных копий для хранения
        /// </summary>
        public int MaxBackupsToKeep { get; set; } = 10;
    }
}
