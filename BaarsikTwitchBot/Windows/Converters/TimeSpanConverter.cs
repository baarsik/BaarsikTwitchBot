using System;
using System.Globalization;
using System.Windows.Data;

namespace BaarsikTwitchBot.Windows.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan timeSpan))
            {
                return new NotSupportedException();
            }

            var totalHours = (int) timeSpan.TotalHours;
            return totalHours > 0
                ? $"{totalHours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}"
                : $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}