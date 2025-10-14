using Library.Services;

namespace Library;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly SemaphoreSlim _dbInitializationLock = new SemaphoreSlim(1, 1);
    private static bool _isDatabaseInitialized = false;

    public App(IServiceProvider serviceProvider)
    {
        System.Diagnostics.Debug.WriteLine("=== App constructor started ===");
        
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("=== App InitializeComponent completed ===");
            
            _serviceProvider = serviceProvider;
            
            // Применяем сохраненные настройки темы
            ApplySavedThemePreference();
            System.Diagnostics.Debug.WriteLine("=== Theme preference loaded ===");
            
            // Применение темы в зависимости от системных настроек
            LoadTheme();
            System.Diagnostics.Debug.WriteLine("=== Theme loaded ===");
            
            // Подписываемся на изменение темы
            RequestedThemeChanged += OnRequestedThemeChanged;
            
            System.Diagnostics.Debug.WriteLine("=== App constructor completed ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in App constructor: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CreateWindow started ===");
            
            // Инициализируем БД синхронно перед созданием Shell
            InitializeDatabaseSync();
            System.Diagnostics.Debug.WriteLine("=== Database ready ===");
            
            // Получаем AppShell через DI
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            System.Diagnostics.Debug.WriteLine("=== AppShell obtained from DI ===");
            
            var window = new Window(shell);
            System.Diagnostics.Debug.WriteLine("=== Window created successfully ===");
            
            return window;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in CreateWindow: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }

    private void InitializeDatabaseSync()
    {
        // Быстрая проверка без блокировки
        if (_isDatabaseInitialized)
            return;

        // Блокировка для потокобезопасности
        _dbInitializationLock.Wait();
        try
        {
            // Двойная проверка после получения блокировки
            if (_isDatabaseInitialized)
                return;

            System.Diagnostics.Debug.WriteLine("=== Starting database initialization (sync) ===");
            
            using var scope = _serviceProvider.CreateScope();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            
            // Синхронное выполнение асинхронного метода
            libraryService.InitializeDatabaseAsync().GetAwaiter().GetResult();
            
            _isDatabaseInitialized = true;
            System.Diagnostics.Debug.WriteLine("=== Database initialization completed (sync) ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in database initialization: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw; // Пробрасываем исключение, чтобы приложение не запустилось с нерабочей БД
        }
        finally
        {
            _dbInitializationLock.Release();
        }
    }

    private void ApplySavedThemePreference()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<SettingsService>();
            var savedTheme = settingsService.GetThemePreference();
            
            // Устанавливаем сохраненную тему
            UserAppTheme = savedTheme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified // Auto
            };
            
            System.Diagnostics.Debug.WriteLine($"=== Applied saved theme preference: {savedTheme} ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Error loading theme preference: {ex.Message} ===");
            // В случае ошибки используем автоматический режим
            UserAppTheme = AppTheme.Unspecified;
        }
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"=== Theme changed to {e.RequestedTheme} ===");
        LoadTheme();
    }

    private void LoadTheme()
    {
        // Получаем текущую системную тему
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        
        try
        {
            // Удаляем старые цветовые схемы если они есть
            var toRemove = Resources.MergedDictionaries
                .Where(d => d.GetType().Name.Contains("Colors"))
                .ToList();
            
            foreach (var dict in toRemove)
            {
                Resources.MergedDictionaries.Remove(dict);
            }
            
            // Загружаем новую цветовую схему
            ResourceDictionary colorDictionary;
            
            if (currentTheme == AppTheme.Dark)
            {
                colorDictionary = new Resources.Styles.ColorsDark();
            }
            else
            {
                colorDictionary = new Resources.Styles.ColorsLight();
            }
            
            Resources.MergedDictionaries.Add(colorDictionary);
            
            System.Diagnostics.Debug.WriteLine($"=== Loaded {currentTheme} theme ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Error loading theme: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
        }
    }
}
