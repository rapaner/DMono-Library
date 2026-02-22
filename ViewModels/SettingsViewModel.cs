using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Services;
using Library.Views;

namespace Library.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly LibraryService _libraryService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private string _themeDescription = "Тема будет автоматически переключаться в зависимости от настроек системы";

    [ObservableProperty]
    private string _appVersion = "Версия 1.0";

    public SettingsViewModel(LibraryService libraryService, SettingsService settingsService)
    {
        _libraryService = libraryService;
        _settingsService = settingsService;

        _appVersion = $"Версия {_settingsService.GetAppVersion()}";
        LoadThemePreference();
    }

    public void LoadThemePreference()
    {
        var savedTheme = _settingsService.GetThemePreference();

        SelectedThemeIndex = savedTheme switch
        {
            "Auto" => 0,
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };

        UpdateThemeDescription(SelectedThemeIndex);
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        if (value == -1) return;

        var selectedTheme = value switch
        {
            0 => AppTheme.Unspecified,
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        var themeKey = value switch
        {
            0 => "Auto",
            1 => "Light",
            2 => "Dark",
            _ => "Auto"
        };
        _settingsService.SaveThemePreference(themeKey);

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = selectedTheme;
        }

        UpdateThemeDescription(value);
    }

    private void UpdateThemeDescription(int selectedIndex)
    {
        ThemeDescription = selectedIndex switch
        {
            0 => "Тема будет автоматически переключаться в зависимости от настроек системы",
            1 => "Всегда использовать светлую тему независимо от настроек системы",
            2 => "Всегда использовать темную тему независимо от настроек системы",
            _ => "Тема будет автоматически переключаться в зависимости от настроек системы"
        };
    }

    [RelayCommand]
    private async Task ClearDataAsync()
    {
        bool result = await Shell.Current.DisplayAlertAsync("Подтверждение",
            "Вы уверены, что хотите удалить все данные? Это действие нельзя отменить!\n\nРекомендуется создать резервную копию на Яндекс Диске перед удалением.",
            "Да, удалить", "Отмена");

        if (result)
        {
            try
            {
                var allBooks = await _libraryService.GetAllBooksAsync();
                foreach (var book in allBooks)
                {
                    await _libraryService.DeleteBookAsync(book);
                }

                await Shell.Current.DisplayAlertAsync("Успех", "Все данные удалены!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task GoToYandexDiskAsync()
    {
        await Shell.Current.GoToAsync(nameof(YandexDiskPage));
    }
}
