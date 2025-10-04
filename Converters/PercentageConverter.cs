using System.Globalization;

namespace Library.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                return percentage / 100.0;
            }
            return 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress * 100.0;
            }
            return 0.0;
        }
    }
}
