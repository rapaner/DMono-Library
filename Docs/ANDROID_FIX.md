# Исправление черного экрана на Android

## Проблема
При запуске приложения на Android появлялся черный экран, интерфейс не загружался, метод `CreateMauiApp` не вызывался.

## Причины
1. **Отсутствие MainApplication.cs** - критический файл для инициализации MAUI приложения на Android
2. **Несоответствие идентификаторов приложения** - в `AndroidManifest.xml` был указан `ru.rapaner.library`, а в `.csproj` - `ru.shuffler149.library`
3. **Неправильная зависимость DI** - `AppShell` (Singleton) зависел от `LibraryService` (Scoped)
4. **Отсутствующие файлы шрифтов** - регистрация несуществующих шрифтов OpenSans могла вызвать сбой на Android

## Внесенные исправления

### 1. Создан файл `Platforms/Android/MainApplication.cs`
```csharp
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        System.Diagnostics.Debug.WriteLine("=== MainApplication constructor called ===");
    }

    protected override MauiApp CreateMauiApp()
    {
        System.Diagnostics.Debug.WriteLine("=== MainApplication.CreateMauiApp called ===");
        return MauiProgram.CreateMauiApp();
    }
}
```

### 2. Исправлен `AndroidManifest.xml`
Изменен package с `ru.rapaner.library` на `ru.shuffler149.library` для соответствия с `.csproj`

### 3. Исправлен `AppShell.xaml.cs`
Удалена зависимость от `LibraryService`, так как инициализация БД уже выполняется в `App.xaml.cs`

### 4. Закомментирована регистрация отсутствующих шрифтов
В `MauiProgram.cs` временно отключена регистрация шрифтов OpenSans, так как файлы отсутствуют в `Resources/Fonts`

### 5. Добавлено диагностическое логирование
Во все критические точки инициализации добавлены `Debug.WriteLine` для отслеживания процесса запуска:
- `MainApplication.cs`
- `MauiProgram.cs`
- `App.xaml.cs`
- `AppShell.xaml.cs`

## Как проверить работу

1. **Пересоберите проект для Android:**
   - Clean Solution
   - Rebuild Solution

2. **Запустите на Android устройстве/эмуляторе**

3. **Проверьте логи в Output Window:**
   Должны появиться сообщения в следующем порядке:
   ```
   === MainApplication constructor called ===
   === MainApplication.CreateMauiApp called ===
   === CreateMauiApp started ===
   === MAUI builder configured ===
   === Database path: ... ===
   === Services registered ===
   === CreateMauiApp completed successfully ===
   === App constructor started ===
   === App InitializeComponent completed ===
   === App constructor completed ===
   === CreateWindow started ===
   === AppShell constructor started ===
   === AppShell InitializeComponent completed ===
   === AppShell routes registered ===
   === AppShell constructor completed ===
   === AppShell obtained from DI ===
   === Window created successfully ===
   ```

## Если проблема сохраняется

1. Проверьте логи Android через **View → Output → Show output from: Debug**
2. Посмотрите логи устройства через **adb logcat**
3. Убедитесь, что все NuGet пакеты установлены корректно
4. Проверьте, что в `Resources/Fonts` есть файлы `OpenSans-Regular.ttf` и `OpenSans-Semibold.ttf`
5. Попробуйте удалить папки `bin` и `obj`, затем пересобрать проект

## Дополнительные проверки

- Убедитесь, что установлен правильный Android SDK (минимум API 21)
- Проверьте, что эмулятор/устройство имеет достаточно памяти
- Проверьте права доступа в `AndroidManifest.xml`
- **Важно**: Если хотите использовать кастомные шрифты, добавьте файлы `.ttf` в папку `Resources/Fonts` и раскомментируйте строки в `MauiProgram.cs`

