using Library.Services;

namespace Library.Views;

public partial class SettingsPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private readonly SettingsService _settingsService;

    public SettingsPage(LibraryService libraryService, SettingsService settingsService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _settingsService = settingsService;
        
        LoadThemePreference();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadThemePreference();
    }
    
    private void LoadThemePreference()
    {
        // Загружаем сохраненные настройки темы
        var savedTheme = _settingsService.GetThemePreference();
        
        // Устанавливаем выбранный элемент в Picker
        ThemePicker.SelectedIndex = savedTheme switch
        {
            "Auto" => 0,
            "Light" => 1,
            "Dark" => 2,
            _ => 0 // По умолчанию Автоматически
        };
        
        UpdateThemeDescription(ThemePicker.SelectedIndex);
    }
    
    private void OnThemeChanged(object sender, EventArgs e)
    {
        if (ThemePicker.SelectedIndex == -1)
            return;
            
        var selectedTheme = ThemePicker.SelectedIndex switch
        {
            0 => AppTheme.Unspecified, // Автоматически
            1 => AppTheme.Light,        // Светлая
            2 => AppTheme.Dark,         // Темная
            _ => AppTheme.Unspecified
        };
        
        // Сохраняем выбор пользователя
        var themeKey = ThemePicker.SelectedIndex switch
        {
            0 => "Auto",
            1 => "Light",
            2 => "Dark",
            _ => "Auto"
        };
        _settingsService.SaveThemePreference(themeKey);
        
        // Применяем тему
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = selectedTheme;
        }
        
        // Обновляем описание
        UpdateThemeDescription(ThemePicker.SelectedIndex);
    }
    
    private void UpdateThemeDescription(int selectedIndex)
    {
        ThemeDescriptionLabel.Text = selectedIndex switch
        {
            0 => "Тема будет автоматически переключаться в зависимости от настроек системы",
            1 => "Всегда использовать светлую тему независимо от настроек системы",
            2 => "Всегда использовать темную тему независимо от настроек системы",
            _ => "Тема будет автоматически переключаться в зависимости от настроек системы"
        };
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтверждение", 
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
                
                await DisplayAlert("Успех", "Все данные удалены!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }
    }

    private async void OnYandexDiskClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(YandexDiskPage));
    }
}
