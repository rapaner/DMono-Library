using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class YandexDiskViewModel : ObservableObject
{
    private readonly YandexDiskService _yandexDiskService;
    private readonly YandexOAuthService _oauthService;
    private readonly SettingsService _settingsService;
    private readonly AppConfiguration _appConfig;
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private string _statusText = "Не подключено";

    [ObservableProperty]
    private string _diskInfoText = string.Empty;

    [ObservableProperty]
    private bool _isDiskInfoVisible;

    [ObservableProperty]
    private string _tokenText = string.Empty;

    [ObservableProperty]
    private string _lastBackupText = "Последняя резервная копия: никогда";

    [ObservableProperty]
    private bool _isAutoBackupEnabled;

    [ObservableProperty]
    private string _frequencyText = "7";

    [ObservableProperty]
    private bool _isFrequencyVisible;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isNoBackupsVisible;

    [ObservableProperty]
    private ObservableCollection<YandexDisk.Client.Protocol.Resource> _backups = new();

    private YandexDisk.Client.Protocol.Resource? _selectedBackup;

    public YandexDiskViewModel(YandexDiskService yandexDiskService, YandexOAuthService oauthService, SettingsService settingsService, AppConfiguration appConfig, IDialogService dialog)
    {
        _yandexDiskService = yandexDiskService;
        _oauthService = oauthService;
        _settingsService = settingsService;
        _appConfig = appConfig;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        var settings = _settingsService.GetYandexDiskSettings();

        if (!string.IsNullOrEmpty(settings.OAuthToken))
        {
            _yandexDiskService.SetOAuthToken(settings.OAuthToken);
            await UpdateStatusAsync();
        }

        IsAutoBackupEnabled = settings.AutoBackupEnabled;
        FrequencyText = settings.AutoBackupFrequencyDays.ToString();
        IsFrequencyVisible = settings.AutoBackupEnabled;

        if (settings.LastBackupDate.HasValue)
            LastBackupText = $"Последняя резервная копия: {settings.LastBackupDate.Value:dd.MM.yyyy HH:mm}";

        if (_yandexDiskService.IsAuthorized)
            await LoadBackupsAsync();
    }

    private async Task UpdateStatusAsync()
    {
        try
        {
            if (_yandexDiskService.IsAuthorized)
            {
                var diskInfo = await _yandexDiskService.GetDiskInfoAsync();
                StatusText = "Подключено";
                var totalGB = diskInfo.TotalSpace / (1024.0 * 1024.0 * 1024.0);
                var usedGB = diskInfo.UsedSpace / (1024.0 * 1024.0 * 1024.0);
                var freeGB = totalGB - usedGB;
                DiskInfoText = $"Использовано: {usedGB:F2} ГБ из {totalGB:F2} ГБ (Свободно: {freeGB:F2} ГБ)";
                IsDiskInfoVisible = true;
            }
            else
            {
                StatusText = "Не подключено";
                IsDiskInfoVisible = false;
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка подключения: {ex.Message}";
            IsDiskInfoVisible = false;
        }
    }

    [RelayCommand]
    private async Task GetTokenAsync()
    {
        try
        {
            IsLoading = true;
            await _oauthService.AuthenticateAsync();
            await _dialog.ShowAlertAsync(
                "Инструкции по получению токена",
                "В открывшемся окне браузера:\n\n1. Войдите в свой аккаунт Яндекс\n2. Разрешите доступ приложению\n3. Скопируйте токен из адресной строки (после #access_token=)\n4. Вставьте токен в поле ниже и нажмите 'Сохранить токен'",
                "Понятно");
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось открыть браузер для авторизации: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveTokenAsync()
    {
        var token = TokenText?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            await _dialog.ShowAlertAsync("Ошибка", "Введите OAuth токен", "OK");
            return;
        }

        try
        {
            _yandexDiskService.SetOAuthToken(token);
            await _yandexDiskService.GetDiskInfoAsync();

            var settings = _settingsService.GetYandexDiskSettings();
            settings.OAuthToken = token;
            _settingsService.SaveYandexDiskSettings(settings);

            await UpdateStatusAsync();
            await LoadBackupsAsync();

            await _dialog.ShowAlertAsync("Успех", "OAuth токен сохранен и проверен", "OK");
            TokenText = string.Empty;
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось подключиться к Яндекс Диску: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        var confirm = await _dialog.ShowConfirmAsync("Подтверждение", "Вы уверены, что хотите отключить Яндекс Диск?", "Да", "Нет");
        if (confirm)
        {
            _settingsService.ClearYandexDiskSettings();
            StatusText = "Не подключено";
            IsDiskInfoVisible = false;
            Backups.Clear();
            IsNoBackupsVisible = false;
            await _dialog.ShowAlertAsync("Успех", "Яндекс Диск отключен", "OK");
        }
    }

    [RelayCommand]
    private async Task BackupAsync()
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Сначала подключите Яндекс Диск", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            if (!File.Exists(_appConfig.DatabasePath))
            {
                await _dialog.ShowAlertAsync("Ошибка", $"Файл базы данных не найден:\n{_appConfig.DatabasePath}", "OK");
                return;
            }

            var success = await _yandexDiskService.BackupDatabaseAsync(_appConfig.DatabasePath);
            if (success)
            {
                var settings = _settingsService.GetYandexDiskSettings();
                settings.LastBackupDate = DateTime.Now;
                _settingsService.SaveYandexDiskSettings(settings);

                LastBackupText = $"Последняя резервная копия: {DateTime.Now:dd.MM.yyyy HH:mm}";
                await _dialog.ShowAlertAsync("Успех", "Резервная копия создана", "OK");
                await LoadBackupsAsync();
            }
            else
            {
                await _dialog.ShowAlertAsync("Ошибка", "Не удалось создать резервную копию", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось создать резервную копию: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Сначала подключите Яндекс Диск", "OK");
            return;
        }

        if (_selectedBackup == null)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Выберите резервную копию для восстановления", "OK");
            return;
        }

        var confirm = await _dialog.ShowConfirmAsync("Подтверждение",
            $"Вы уверены, что хотите восстановить базу данных из резервной копии?\n\n{_selectedBackup.Name}\n\nТекущая база данных будет заменена.",
            "Да", "Нет");
        if (!confirm) return;

        try
        {
            IsLoading = true;
            var success = await _yandexDiskService.RestoreDatabaseAsync(_selectedBackup.Path, _appConfig.DatabasePath);
            if (success)
            {
                await _dialog.ShowAlertAsync("Успех", "База данных восстановлена из резервной копии.\n\nПриложение нужно перезапустить.", "OK");
                Application.Current?.Quit();
            }
            else
            {
                await _dialog.ShowAlertAsync("Ошибка", "Не удалось восстановить базу данных", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось восстановить базу данных: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBackupAsync()
    {
        if (!_yandexDiskService.IsAuthorized)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Сначала подключите Яндекс Диск", "OK");
            return;
        }

        if (_selectedBackup == null)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Выберите резервную копию для удаления", "OK");
            return;
        }

        var confirm = await _dialog.ShowConfirmAsync("Подтверждение",
            $"Вы уверены, что хотите удалить резервную копию?\n\n{_selectedBackup.Name}\n\nЭто действие нельзя отменить.",
            "Да", "Нет");
        if (!confirm) return;

        try
        {
            IsLoading = true;
            var success = await _yandexDiskService.DeleteFileAsync(_selectedBackup.Path, permanently: true);
            if (success)
            {
                await _dialog.ShowAlertAsync("Успех", "Резервная копия удалена", "OK");
                _selectedBackup = null;
                await LoadBackupsAsync();
            }
            else
            {
                await _dialog.ShowAlertAsync("Ошибка", "Не удалось удалить резервную копию", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось удалить резервную копию: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AutoBackupToggled(bool isEnabled)
    {
        IsFrequencyVisible = isEnabled;
        var settings = _settingsService.GetYandexDiskSettings();
        settings.AutoBackupEnabled = isEnabled;
        _settingsService.SaveYandexDiskSettings(settings);
    }

    [RelayCommand]
    private async Task SaveFrequencyAsync()
    {
        if (int.TryParse(FrequencyText, out int frequency) && frequency > 0)
        {
            var settings = _settingsService.GetYandexDiskSettings();
            settings.AutoBackupFrequencyDays = frequency;
            _settingsService.SaveYandexDiskSettings(settings);
            await _dialog.ShowAlertAsync("Успех", $"Частота резервного копирования установлена: {frequency} дней", "OK");
        }
        else
        {
            await _dialog.ShowAlertAsync("Ошибка", "Введите корректное число дней (больше 0)", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshBackupsAsync()
    {
        await LoadBackupsAsync();
    }

    public void OnBackupSelected(YandexDisk.Client.Protocol.Resource? backup)
    {
        _selectedBackup = backup;
    }

    private async Task LoadBackupsAsync()
    {
        if (!_yandexDiskService.IsAuthorized) return;

        try
        {
            IsLoading = true;
            var backups = await _yandexDiskService.GetBackupListAsync();

            Backups = new ObservableCollection<YandexDisk.Client.Protocol.Resource>(backups);
            IsNoBackupsVisible = !backups.Any();
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось загрузить список резервных копий: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
