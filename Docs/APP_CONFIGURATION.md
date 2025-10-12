# AppConfiguration - Конфигурация приложения

## Обзор

Класс `AppConfiguration` содержит все настройки и пути приложения. Регистрируется как singleton в DI контейнере и используется во всех сервисах и страницах.

## Расположение

`Models/AppConfiguration.cs`

## Свойства

### DatabasePath
- **Тип**: `string`
- **Описание**: Полный путь к файлу базы данных
- **Пример**: `/data/user/0/ru.rapaner.library/files/library.db`

### DatabaseFileName
- **Тип**: `string`
- **Описание**: Имя файла базы данных
- **По умолчанию**: `library.db`

### AppDataDirectory
- **Тип**: `string`
- **Описание**: Директория для хранения данных приложения
- **Пример**: `/data/user/0/ru.rapaner.library/files`

### AppVersion
- **Тип**: `string`
- **Описание**: Версия приложения (из `AppInfo.VersionString`)
- **Пример**: `1.8`

### AppName
- **Тип**: `string`
- **Описание**: Имя приложения (из `AppInfo.Name`)
- **По умолчанию**: `Library`

## Инициализация

Конфигурация создаётся и регистрируется в `MauiProgram.cs`:

```csharp
var appConfig = new AppConfiguration
{
    AppDataDirectory = FileSystem.AppDataDirectory,
    DatabaseFileName = "library.db",
    DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "library.db"),
    AppVersion = AppInfo.VersionString,
    AppName = AppInfo.Name
};

builder.Services.AddSingleton(appConfig);
```

## Использование

### В сервисах

```csharp
public class LibraryService
{
    private readonly string _dbPath;
    
    public LibraryService(LibraryDbContext context, AppConfiguration appConfig)
    {
        _context = context;
        _dbPath = appConfig.DatabasePath;
    }
}
```

### На страницах

```csharp
public partial class YandexDiskPage : ContentPage
{
    private readonly AppConfiguration _appConfig;
    
    public YandexDiskPage(YandexDiskService service, AppConfiguration appConfig)
    {
        _appConfig = appConfig;
        
        // Использование пути к БД
        await service.BackupDatabaseAsync(_appConfig.DatabasePath);
    }
}
```

### Через DI

```csharp
// В конструкторе
private readonly AppConfiguration _appConfig;

public MyClass(AppConfiguration appConfig)
{
    _appConfig = appConfig;
}

// Использование
var dbPath = _appConfig.DatabasePath;
var version = _appConfig.AppVersion;
```

## Преимущества

1. **Типобезопасность**: Вместо строк используется объект с типизированными свойствами
2. **Единый источник правды**: Все пути и настройки в одном месте
3. **Расширяемость**: Легко добавить новые настройки без изменения сигнатур методов
4. **Тестируемость**: Легко создать mock конфигурацию для тестов
5. **Документируемость**: Все свойства имеют XML комментарии

## Добавление новых настроек

1. Добавьте свойство в класс `AppConfiguration`:
```csharp
/// <summary>
/// Максимальный размер кэша в МБ
/// </summary>
public int MaxCacheSizeMb { get; set; } = 100;
```

2. Инициализируйте значение в `MauiProgram.cs`:
```csharp
var appConfig = new AppConfiguration
{
    // ... существующие настройки
    MaxCacheSizeMb = 100
};
```

3. Используйте в коде:
```csharp
var maxSize = _appConfig.MaxCacheSizeMb;
```

## Рекомендации

- Не изменяйте значения свойств после создания (treat as immutable)
- Для изменяемых настроек пользователя используйте `SettingsService`
- `AppConfiguration` - для статических настроек приложения
- Добавляйте XML комментарии ко всем новым свойствам

