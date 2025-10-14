using Library.Services;
using Library.Core.Models;
using Library.Models;
using YandexDisk.Client.Protocol;

namespace Library.Views
{
    public partial class YandexDiskPage : ContentPage
    {
        private readonly YandexDiskService _yandexDiskService;
        private readonly YandexOAuthService _oauthService;
        private readonly SettingsService _settingsService;
        private readonly AppConfiguration _appConfig;
        private YandexDisk.Client.Protocol.Resource? _selectedBackup;

        public YandexDiskPage(YandexDiskService yandexDiskService, YandexOAuthService oauthService, SettingsService settingsService, AppConfiguration appConfig)
        {
            InitializeComponent();
            _yandexDiskService = yandexDiskService;
            _oauthService = oauthService;
            _settingsService = settingsService;
            _appConfig = appConfig;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            var settings = _settingsService.GetYandexDiskSettings();

            if (!string.IsNullOrEmpty(settings.OAuthToken))
            {
                _yandexDiskService.SetOAuthToken(settings.OAuthToken);
                await UpdateStatusAsync();
            }

            AutoBackupSwitch.IsToggled = settings.AutoBackupEnabled;
            FrequencyEntry.Text = settings.AutoBackupFrequencyDays.ToString();
            FrequencyLayout.IsVisible = settings.AutoBackupEnabled;

            if (settings.LastBackupDate.HasValue)
            {
                LastBackupLabel.Text = $"Последняя резервная копия: {settings.LastBackupDate.Value:dd.MM.yyyy HH:mm}";
            }

            if (_yandexDiskService.IsAuthorized)
            {
                await LoadBackupsAsync();
            }
        }

        private async Task UpdateStatusAsync()
        {
            try
            {
                if (_yandexDiskService.IsAuthorized)
                {
                    var diskInfo = await _yandexDiskService.GetDiskInfoAsync();
                    StatusLabel.Text = "Подключено";
                    
                    var totalGB = diskInfo.TotalSpace / (1024.0 * 1024.0 * 1024.0);
                    var usedGB = diskInfo.UsedSpace / (1024.0 * 1024.0 * 1024.0);
                    var freeGB = totalGB - usedGB;
                    
                    DiskInfoLabel.Text = $"Использовано: {usedGB:F2} ГБ из {totalGB:F2} ГБ (Свободно: {freeGB:F2} ГБ)";
                    DiskInfoLabel.IsVisible = true;
                }
                else
                {
                    StatusLabel.Text = "Не подключено";
                    DiskInfoLabel.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Ошибка подключения: {ex.Message}";
                DiskInfoLabel.IsVisible = false;
            }
        }

        private async void OnGetTokenClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                // Запускаем OAuth авторизацию через браузер
                var token = await _oauthService.AuthenticateAsync();

                if (!string.IsNullOrEmpty(token))
                {
                    // Автоматически сохраняем полученный токен
                    _yandexDiskService.SetOAuthToken(token);
                    
                    // Проверяем токен
                    await _yandexDiskService.GetDiskInfoAsync();

                    // Сохраняем токен в настройках
                    var settings = _settingsService.GetYandexDiskSettings();
                    settings.OAuthToken = token;
                    _settingsService.SaveYandexDiskSettings(settings);

                    await UpdateStatusAsync();
                    await LoadBackupsAsync();

                    await DisplayAlert("Успех", "Авторизация прошла успешно! Вы подключены к Яндекс Диску.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось выполнить авторизацию: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnSaveTokenClicked(object sender, EventArgs e)
        {
            var token = TokenEntry.Text?.Trim();

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Ошибка", "Введите OAuth токен", "OK");
                return;
            }

            try
            {
                _yandexDiskService.SetOAuthToken(token);
                
                // Проверяем токен, пытаясь получить информацию о диске
                await _yandexDiskService.GetDiskInfoAsync();

                // Сохраняем токен
                var settings = _settingsService.GetYandexDiskSettings();
                settings.OAuthToken = token;
                _settingsService.SaveYandexDiskSettings(settings);

                await UpdateStatusAsync();
                await LoadBackupsAsync();

                await DisplayAlert("Успех", "OAuth токен сохранен и проверен", "OK");
                TokenEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось подключиться к Яндекс Диску: {ex.Message}", "OK");
            }
        }

        private async void OnDisconnectClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Подтверждение",
                "Вы уверены, что хотите отключить Яндекс Диск?",
                "Да",
                "Нет");

            if (confirm)
            {
                _settingsService.ClearYandexDiskSettings();
                StatusLabel.Text = "Не подключено";
                DiskInfoLabel.IsVisible = false;
                BackupsCollectionView.ItemsSource = null;
                NoBackupsLabel.IsVisible = false;
                
                await DisplayAlert("Успех", "Яндекс Диск отключен", "OK");
            }
        }

        private async void OnBackupClicked(object sender, EventArgs e)
        {
            if (!_yandexDiskService.IsAuthorized)
            {
                await DisplayAlert("Ошибка", "Сначала подключите Яндекс Диск", "OK");
                return;
            }

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                // Проверяем существование файла БД
                if (!File.Exists(_appConfig.DatabasePath))
                {
                    await DisplayAlert("Ошибка", $"Файл базы данных не найден:\n{_appConfig.DatabasePath}", "OK");
                    System.Diagnostics.Debug.WriteLine($"=== Database file not found: {_appConfig.DatabasePath} ===");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"=== Starting backup of: {_appConfig.DatabasePath} ===");
                System.Diagnostics.Debug.WriteLine($"=== File size: {new FileInfo(_appConfig.DatabasePath).Length} bytes ===");

                var success = await _yandexDiskService.BackupDatabaseAsync(_appConfig.DatabasePath);

                if (success)
                {
                    var settings = _settingsService.GetYandexDiskSettings();
                    settings.LastBackupDate = DateTime.Now;
                    _settingsService.SaveYandexDiskSettings(settings);

                    LastBackupLabel.Text = $"Последняя резервная копия: {DateTime.Now:dd.MM.yyyy HH:mm}";

                    await DisplayAlert("Успех", "Резервная копия создана", "OK");
                    await LoadBackupsAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось создать резервную копию", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось создать резервную копию: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            if (!_yandexDiskService.IsAuthorized)
            {
                await DisplayAlert("Ошибка", "Сначала подключите Яндекс Диск", "OK");
                return;
            }

            if (_selectedBackup == null)
            {
                await DisplayAlert("Ошибка", "Выберите резервную копию для восстановления", "OK");
                return;
            }

            var confirm = await DisplayAlert(
                "Подтверждение",
                $"Вы уверены, что хотите восстановить базу данных из резервной копии?\n\n{_selectedBackup.Name}\n\nТекущая база данных будет заменена.",
                "Да",
                "Нет");

            if (!confirm)
                return;

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                var success = await _yandexDiskService.RestoreDatabaseAsync(_selectedBackup.Path, _appConfig.DatabasePath);

                if (success)
                {
                    await DisplayAlert(
                        "Успех",
                        "База данных восстановлена из резервной копии.\n\nПриложение нужно перезапустить.",
                        "OK");

                    // Перезапуск приложения
                    Application.Current?.Quit();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось восстановить базу данных", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось восстановить базу данных: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private void OnAutoBackupToggled(object sender, ToggledEventArgs e)
        {
            FrequencyLayout.IsVisible = e.Value;

            var settings = _settingsService.GetYandexDiskSettings();
            settings.AutoBackupEnabled = e.Value;
            _settingsService.SaveYandexDiskSettings(settings);
        }

        private void OnSaveFrequencyClicked(object sender, EventArgs e)
        {
            if (int.TryParse(FrequencyEntry.Text, out int frequency) && frequency > 0)
            {
                var settings = _settingsService.GetYandexDiskSettings();
                settings.AutoBackupFrequencyDays = frequency;
                _settingsService.SaveYandexDiskSettings(settings);

                DisplayAlert("Успех", $"Частота резервного копирования установлена: {frequency} дней", "OK");
            }
            else
            {
                DisplayAlert("Ошибка", "Введите корректное число дней (больше 0)", "OK");
            }
        }

        private async void OnRefreshBackupsClicked(object sender, EventArgs e)
        {
            await LoadBackupsAsync();
        }

        private async Task LoadBackupsAsync()
        {
            if (!_yandexDiskService.IsAuthorized)
                return;

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                var backups = await _yandexDiskService.GetBackupListAsync();

                if (backups.Any())
                {
                    BackupsCollectionView.ItemsSource = backups;
                    NoBackupsLabel.IsVisible = false;
                }
                else
                {
                    BackupsCollectionView.ItemsSource = null;
                    NoBackupsLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить список резервных копий: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private void OnBackupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is YandexDisk.Client.Protocol.Resource backup)
            {
                _selectedBackup = backup;
                DisplayAlert("Резервная копия выбрана", $"Выбрана резервная копия:\n{backup.Name}\n\nНажмите 'Восстановить из резервной копии' для восстановления.", "OK");
            }
        }
    }
}
