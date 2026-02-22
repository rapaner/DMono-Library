namespace Library.Extensions;

public static class ApplicationExtensions
{
    public static Color GetThemeColor(this Application? app, string resourceKey, Color defaultColor)
    {
        if (app?.Resources.TryGetValue(resourceKey, out var color) == true && color is Color themeColor)
            return themeColor;
        return defaultColor;
    }
}
