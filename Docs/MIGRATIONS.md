# Миграции Entity Framework Core

## Обзор

Проект использует Entity Framework Core с SQLite для хранения данных библиотеки книг. Система миграций позволяет безопасно изменять структуру базы данных при развитии приложения.

## Структура миграций

- `Migrations/` - папка с файлами миграций
- `Migrations/LibraryDbContextModelSnapshot.cs` - снимок текущей модели данных
- `Data/LibraryDbContextFactory.cs` - фабрика для создания контекста во время разработки

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

1. Создать новый файл в папке `Migrations/` с именем `YYYYMMDDHHMMSS_MigrationName.cs`
2. Реализовать методы `Up()` и `Down()` в классе миграции
3. Обновить `LibraryDbContextModelSnapshot.cs` с новой структурой

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

Миграции автоматически применяются при запуске приложения через метод `EnsureDatabaseCreatedAsync()` в `LibraryService`.

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
