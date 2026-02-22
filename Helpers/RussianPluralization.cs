namespace Library.Helpers;

public static class RussianPluralization
{
    public static string Pluralize(int count, string one, string few, string many)
    {
        var lastTwoDigits = count % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 14) return many;
        return (count % 10) switch { 1 => one, 2 or 3 or 4 => few, _ => many };
    }

    public static string Days(int count) => Pluralize(count, "день", "дня", "дней");

    public static string Months(int count) => Pluralize(count, "месяц", "месяца", "месяцев");
}
