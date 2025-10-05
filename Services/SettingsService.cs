using Library.Models;
using System.Text.Json;

namespace Library.Services
{
    /// <summary>
    /// Сервис для работы с настройками приложения
    /// </summary>
    public class SettingsService
    {
        private const string YandexDiskSettingsKey = "YandexDiskSettings";

        /// <summary>
        /// Получить настройки Яндекс Диска
        /// </summary>
        /// <returns>Настройки Яндекс Диска</returns>
        public YandexDiskSettings GetYandexDiskSettings()
        {
            var json = Preferences.Get(YandexDiskSettingsKey, string.Empty);
            
            if (string.IsNullOrEmpty(json))
            {
                return new YandexDiskSettings();
            }

            try
            {
                return JsonSerializer.Deserialize<YandexDiskSettings>(json) ?? new YandexDiskSettings();
            }
            catch
            {
                return new YandexDiskSettings();
            }
        }

        /// <summary>
        /// Сохранить настройки Яндекс Диска
        /// </summary>
        /// <param name="settings">Настройки для сохранения</param>
        public void SaveYandexDiskSettings(YandexDiskSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            Preferences.Set(YandexDiskSettingsKey, json);
        }

        /// <summary>
        /// Очистить настройки Яндекс Диска
        /// </summary>
        public void ClearYandexDiskSettings()
        {
            Preferences.Remove(YandexDiskSettingsKey);
        }
    }
}
