namespace Library.Models;

/// <summary>
/// Вариант фильтра по полке на странице выбора книги.
/// </summary>
public record BookChooseShelfOption(int? ShelfId, string Name);
