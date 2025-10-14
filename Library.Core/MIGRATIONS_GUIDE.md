# Руководство по работе с миграциями в Library.Core

## Структура проекта

Проект **Library.Core** содержит все модели базы данных и миграции Entity Framework Core:

```
Library.Core/
├── Data/
│   ├── LibraryDbContext.cs          # Контекст БД
│   ├── LibraryDbContextFactory.cs   # Фабрика для design-time (для dotnet ef)
│   └── Migrations/                  # Папка с миграциями
├── Models/                          # Модели-сущности БД
│   ├── Book.cs
│   ├── Book.Computed.cs
│   ├── Author.cs
│   ├── PagesReadInDate.cs
│   ├── BookReadingSchedule.cs
│   └── BookStatus.cs
└── Library.Core.csproj
```

## Требования

- **.NET SDK 9.0** или выше
- **dotnet-ef tools** (глобальный инструмент EF Core)

### Установка dotnet-ef (если не установлен)

```powershell
dotnet tool install --global dotnet-ef
```

Или обновление до последней версии:

```powershell
dotnet tool update --global dotnet-ef
```

Проверка установки:

```powershell
dotnet ef --version
```

## Работа с миграциями

### 1. Просмотр существующих миграций

Перейдите в папку Library.Core:

```powershell
cd "E:\VS Projects\MAUI\Library\Library.Core"
```

Посмотрите список всех миграций:

```powershell
dotnet ef migrations list
```

Результат покажет:
- ✓ Применённые миграции
- Pending - ожидающие миграции

### 2. Создание новой миграции

#### Шаг 1: Внесите изменения в модели

Например, добавьте новое свойство в класс `Book`:

```csharp
// Models/Book.cs
public string? Publisher { get; set; }
```

#### Шаг 2: Создайте миграцию

Находясь в папке `Library.Core`, выполните:

```powershell
dotnet ef migrations add <ИмяМиграции>
```

Примеры имён миграций:
```powershell
# Добавление нового поля
dotnet ef migrations add AddPublisherToBook

# Добавление новой таблицы
dotnet ef migrations add AddGenreTable

# Изменение существующей структуры
dotnet ef migrations add UpdateBookIndexes
```

**Правила именования:**
- Используйте PascalCase
- Описывайте, ЧТО изменяется (глагол + существительное)
- Примеры: `AddPublisher`, `RemoveOldField`, `UpdateIndexes`

#### Шаг 3: Проверьте созданные файлы

После создания миграции в папке `Data/Migrations/` появятся файлы:
- `<timestamp>_<ИмяМиграции>.cs` - код миграции
- `LibraryDbContextModelSnapshot.cs` - обновлённый snapshot модели

### 3. Удаление последней миграции

Если допущена ошибка в миграции (ещё не применённой к БД):

```powershell
dotnet ef migrations remove
```

⚠️ **Внимание:** Удаляется только последняя миграция, которая ещё НЕ применена к базе данных!

### 4. Применение миграций (в runtime)

Миграции применяются автоматически при запуске приложения Library через `DatabaseMigrationService`.

Для ручного применения (если нужно):

```powershell
dotnet ef database update
```

Применить до конкретной миграции:

```powershell
dotnet ef database update <ИмяМиграции>
```

Откатить все миграции:

```powershell
dotnet ef database update 0
```

### 5. Генерация SQL-скрипта

Для просмотра SQL-кода миграции без применения:

```powershell
# Скрипт от текущего состояния до последней миграции
dotnet ef migrations script

# Скрипт от начала до конкретной миграции
dotnet ef migrations script 0 AddPublisherToBook

# Сохранить в файл
dotnet ef migrations script > migration.sql
```

## Типичные сценарии

### Сценарий 1: Добавление нового поля в существующую таблицу

1. Откройте модель (например, `Models/Book.cs`)
2. Добавьте новое свойство:
   ```csharp
   public string? Publisher { get; set; }
   ```
3. Создайте миграцию:
   ```powershell
   cd "E:\VS Projects\MAUI\Library\Library.Core"
   dotnet ef migrations add AddPublisherToBook
   ```
4. Проверьте сгенерированный код миграции
5. Запустите приложение - миграция применится автоматически

### Сценарий 2: Добавление новой таблицы

1. Создайте новую модель в `Models/`:
   ```csharp
   // Models/Genre.cs
   public record Genre
   {
       [Key]
       public int Id { get; set; }
       
       [Required]
       [MaxLength(50)]
       public string Name { get; set; } = string.Empty;
       
       public ICollection<Book> Books { get; set; } = new List<Book>();
   }
   ```

2. Добавьте DbSet в `LibraryDbContext.cs`:
   ```csharp
   public DbSet<Genre> Genres { get; set; }
   ```

3. Настройте связь в `OnModelCreating`:
   ```csharp
   modelBuilder.Entity<Genre>(entity =>
   {
       entity.HasKey(e => e.Id);
       entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
   });
   ```

4. Создайте миграцию:
   ```powershell
   dotnet ef migrations add AddGenreTable
   ```

### Сценарий 3: Изменение существующего поля

1. Измените свойство в модели:
   ```csharp
   // Было:
   [MaxLength(100)]
   public string Title { get; set; }
   
   // Стало:
   [MaxLength(300)]
   public string Title { get; set; }
   ```

2. Создайте миграцию:
   ```powershell
   dotnet ef migrations add IncreaseTitleLength
   ```

## Устранение неполадок

### Проблема: "Build failed"

**Решение:** Убедитесь, что проект собирается без ошибок:

```powershell
dotnet build
```

Исправьте ошибки компиляции перед созданием миграции.

### Проблема: "No DbContext was found"

**Решение:** Убедитесь, что:
1. Вы находитесь в папке `Library.Core`
2. Существует `LibraryDbContextFactory.cs`
3. Проект содержит пакет `Microsoft.EntityFrameworkCore.Design`

### Проблема: "The migration has already been applied"

**Решение:** Нельзя удалить миграцию, которая уже применена к БД. Вместо этого:
1. Создайте новую миграцию с обратными изменениями
2. Или откатите БД: `dotnet ef database update <ПредыдущаяМиграция>`

### Проблема: Конфликт миграций при работе в команде

**Решение:**
1. Получите последние изменения из git
2. Удалите свою локальную неприменённую миграцию: `dotnet ef migrations remove`
3. Пересоздайте миграцию с новым timestamp

## Best Practices

### ✅ Делайте

- **Проверяйте код миграции** перед коммитом
- **Тестируйте миграции** на тестовой БД
- **Используйте описательные имена** для миграций
- **Создавайте отдельную миграцию** для каждого логического изменения
- **Коммитьте миграции вместе** с изменениями моделей

### ❌ Не делайте

- **Не редактируйте** уже применённые миграции
- **Не удаляйте** миграции, которые уже в production
- **Не изменяйте** `LibraryDbContextModelSnapshot.cs` вручную
- **Не создавайте** миграции с одинаковыми именами
- **Не коммитьте** недоработанные миграции

## Интеграция с основным проектом Library

Проект **Library** использует **Library.Core** через ProjectReference:

```xml
<!-- Library/Library.csproj -->
<ItemGroup>
  <ProjectReference Include="Library.Core\Library.Core.csproj" />
</ItemGroup>
```

Миграции применяются автоматически в `DatabaseMigrationService.cs` при старте приложения.

### Настройка сборки миграций

В `MauiProgram.cs` указана сборка с миграциями:

```csharp
var migrationsAssembly = typeof(LibraryDbContext).Assembly.GetName().Name;

builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    options.UseSqlite(
        $"Data Source={appConfig.DatabasePath}",
        sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly));
});
```

Это автоматически использует миграции из **Library.Core.dll**.

## Полезные команды

```powershell
# Переход в папку проекта
cd "E:\VS Projects\MAUI\Library\Library.Core"

# Список миграций
dotnet ef migrations list

# Создать миграцию
dotnet ef migrations add <Name>

# Удалить последнюю миграцию
dotnet ef migrations remove

# Применить миграции
dotnet ef database update

# Откатить до миграции
dotnet ef database update <MigrationName>

# Откатить все миграции
dotnet ef database update 0

# Сгенерировать SQL скрипт
dotnet ef migrations script

# Информация о DbContext
dotnet ef dbcontext info

# Проверить версию EF Tools
dotnet ef --version
```

## Примеры реальных миграций

### Пример 1: AddIsbnToBook

```powershell
cd "E:\VS Projects\MAUI\Library\Library.Core"
dotnet ef migrations add AddIsbnToBook
```

```csharp
// Добавить в Models/Book.cs
[MaxLength(13)]
public string? ISBN { get; set; }
```

### Пример 2: AddBookCover

```powershell
dotnet ef migrations add AddBookCoverImage
```

```csharp
// Добавить в Models/Book.cs
public byte[]? CoverImage { get; set; }
```

### Пример 3: CreateReviewsTable

```powershell
dotnet ef migrations add CreateReviewsTable
```

Создать `Models/Review.cs` и обновить `LibraryDbContext.cs`

---

## Контакты и поддержка

При возникновении вопросов по миграциям:
1. Проверьте логи в Debug Output
2. Проверьте файлы миграций в `Data/Migrations/`
3. Убедитесь, что версии пакетов совместимы (все EF Core пакеты версии 9.0.0)

**Версия документа:** 1.0  
**Дата создания:** 14.10.2025  
**Проект:** Library.Core (.NET 9)

