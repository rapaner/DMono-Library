using Library.Core.Models;

namespace Library.Models;

/// <summary>
/// Вариант фильтра по статусу на странице выбора книги.
/// </summary>
public record BookChooseStatusFilterOption(string Key, string Name, BookStatus? Status)
{
    /// <summary>
    /// Все книги.
    /// </summary>
    public static BookChooseStatusFilterOption All => new("All", "Все", null);

    /// <summary>
    /// Все варианты фильтра.
    /// </summary>
    public static IReadOnlyList<BookChooseStatusFilterOption> GetAll() =>
    [
        All,
        new("Planned", "В планах", BookStatus.Planned),
        new("Current", "Читаю сейчас", BookStatus.Reading),
        new("Finished", "Прочитано", BookStatus.Finished),
        new("FinishedLongAgo", "Прочитана давно", BookStatus.FinishedLongAgo),
    ];
}
