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
        private const string ThemePreferenceKey = "AppThemePreference";
        private const string BookChooseBooksAmountKey = "BookChooseBooksAmount";
        private const string BookChooseLastChosenBookNumberKey = "BookChooseLastChosenBookNumber";
        private const string BookChooseServiceOptionKey = "BookChooseServiceOption";

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

        /// <summary>
        /// Получить сохраненные настройки темы
        /// </summary>
        /// <returns>Настройка темы: "Auto", "Light" или "Dark"</returns>
        public string GetThemePreference()
        {
            return Preferences.Get(ThemePreferenceKey, "Auto");
        }

        /// <summary>
        /// Сохранить настройки темы
        /// </summary>
        /// <param name="theme">Тема: "Auto", "Light" или "Dark"</param>
        public void SaveThemePreference(string theme)
        {
            Preferences.Set(ThemePreferenceKey, theme);
        }

        /// <summary>
        /// Очистить настройки темы (вернуться к автоматическому режиму)
        /// </summary>
        public void ClearThemePreference()
        {
            Preferences.Remove(ThemePreferenceKey);
        }

        /// <summary>
        /// Получить версию приложения
        /// </summary>
        /// <returns>Строка с версией приложения</returns>
        public string GetAppVersion()
        {
            return AppInfo.Current.VersionString;
        }

        /// <summary>
        /// Прочитать настройки выбора книги.
        /// </summary>
        /// <returns>Кортеж (booksAmount, lastChosenBookNumber, currentOption)</returns>
        public (int BooksAmount, int LastChosenBookNumber, int CurrentBookChooseServiceOption) GetBookChooseSettings()
        {
            int booksAmount = Preferences.Get(BookChooseBooksAmountKey, 1000);
            int lastChosen = Preferences.Get(BookChooseLastChosenBookNumberKey, 0);
            int option = Preferences.Get(BookChooseServiceOptionKey, -1);
            return (booksAmount, lastChosen, option);
        }

        /// <summary>
        /// Сохранить настройки выбора книги.
        /// </summary>
        public void SaveBookChooseSettings(int booksAmount, int lastChosenBookNumber, int currentBookChooseServiceOption)
        {
            Preferences.Set(BookChooseBooksAmountKey, booksAmount);
            Preferences.Set(BookChooseLastChosenBookNumberKey, lastChosenBookNumber);
            Preferences.Set(BookChooseServiceOptionKey, currentBookChooseServiceOption);
        }
    }
}