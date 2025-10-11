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
3. **Экспорт в JSON**: Все данные экспортируются в JSON файл `library.db.backup-{timestamp}.json`
4. **Создание бэкапа**: Создаётся файловая резервная копия БД `library.db.backup-{timestamp}` (на случай проблем)
5. **Пересоздание БД**: Старая БД удаляется, создаётся новая через миграции
6. **Импорт из JSON**: Данные импортируются из JSON через EF Core в правильном порядке
7. **Очистка**: JSON и файловый бэкап удаляются после успешного импорта
8. **Завершение**: БД готова к работе с новой структурой миграций

### Безопасность данных

- **Двойной бэкап**: JSON для импорта + файловая копия БД на случай проблем
- **Экспорт до изменений**: Данные сохраняются **до** удаления старой БД
- **Проверка на каждом этапе**: Детальное логирование всех операций
- **Обработка ошибок**: При ошибке импорта бэкапы сохраняются для ручного восстановления
- **Автоматическая очистка**: Временные файлы удаляются только после успешного завершения

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
