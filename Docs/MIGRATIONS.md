# Миграции Entity Framework Core

## Обзор

Проект использует Entity Framework Core с SQLite для хранения данных библиотеки книг. Система миграций позволяет безопасно изменять структуру базы данных при развитии приложения.

**Важно:** Приложение автоматически мигрирует старые базы данных (созданные через `EnsureCreatedAsync`) на новую систему миграций с сохранением всех данных.

## Структура миграций

- `Data/Migrations/` - папка с файлами миграций
- `Data/Migrations/LibraryDbContextModelSnapshot.cs` - снимок текущей модели данных
- `Data/LibraryDbContextFactory.cs` - фабрика для создания контекста во время разработки
- `Services/DatabaseMigrationService.cs` - сервис для управления миграциями и переноса данных

## Создание новой миграции

### Автоматический способ (рекомендуется)

```bash
# Установить EF Core tools (если не установлены)
dotnet tool install --global dotnet-ef

# Создать новую миграцию
dotnet ef migrations add MigrationName --project Library.csproj

# Применить миграцию к базе данных
dotnet ef database update --project Library.csproj
```

### Ручной способ

1. Создать новый файл в папке `Data/Migrations/` с именем `YYYYMMDDHHMMSS_MigrationName.cs`
2. Реализовать методы `Up()` и `Down()` в классе миграции
3. Обновить `Data/Migrations/LibraryDbContextModelSnapshot.cs` с новой структурой

Пример структуры миграции см. в `Data/Migrations/20250101000000_InitialCreate.cs`

## Пример изменения модели

### Добавление нового поля

```csharp
// В модели Book
public string ISBN { get; set; } = string.Empty;

// В миграции
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "ISBN",
        table: "Books",
        type: "TEXT",
        nullable: false,
        defaultValue: "");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "ISBN",
        table: "Books");
}
```

### Изменение существующего поля

```csharp
// В миграции
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "Title",
        table: "Books",
        type: "TEXT",
        maxLength: 300, // Увеличиваем лимит
        nullable: false,
        oldClrType: typeof(string),
        oldType: "TEXT",
        oldMaxLength: 200);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "Title",
        table: "Books",
        type: "TEXT",
        maxLength: 200, // Возвращаем старый лимит
        nullable: false,
        oldClrType: typeof(string),
        oldType: "TEXT",
        oldMaxLength: 300);
}
```

## Применение миграций в приложении

Миграции автоматически применяются при запуске приложения через метод `InitializeDatabaseAsync()` в `LibraryService`.

### Автоматическая миграция существующих баз данных

При первом запуске приложения с новой системой миграций:

1. **Проверка**: Система проверяет наличие таблицы `__EFMigrationsHistory`
2. **Обнаружение старой БД**: Если таблица отсутствует, но файл БД существует
3. **Создание бэкапа**: Создаётся резервная копия с именем `library.db.backup-{timestamp}`
4. **Пересоздание БД**: Старая БД удаляется, создаётся новая через миграции
5. **Восстановление данных**: Данные копируются из бэкапа через SQLite ATTACH DATABASE
6. **Завершение**: БД готова к работе, бэкап сохраняется для возможного отката

### Безопасность данных

- Бэкап создаётся **до** любых изменений в БД
- Используется прямое копирование через SQLite для надёжности
- Все операции логируются в Debug консоль
- При ошибке миграции бэкап остаётся нетронутым

## Рекомендации

1. **Всегда создавайте миграции** при изменении модели данных
2. **Тестируйте миграции** на тестовых данных перед продакшеном
3. **Делайте резервные копии** перед применением миграций
4. **Используйте осмысленные имена** для миграций
5. **Не удаляйте старые миграции** без крайней необходимости

## Откат миграций

```bash
# Откатиться к предыдущей миграции
dotnet ef database update PreviousMigrationName --project Library.csproj

# Откатиться к началу (удалить все таблицы)
dotnet ef database update 0 --project Library.csproj
```

## Устранение проблем

### Конфликты миграций
- Удалите папку `Migrations/`
- Создайте новую начальную миграцию
- Примените её к базе данных

### Проблемы с базой данных
- Удалите файл базы данных
- Пересоздайте базу данных с помощью миграций
