using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BaarsikTwitchBot.Windows.Converters
{
    public class BitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bmp = new BitmapImage();
            switch (value)
            {
                case string valueStr:
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(valueStr, UriKind.RelativeOrAbsolute);
                    bmp.EndInit();
                    return bmp;
                case Uri valueUri:
                    bmp.BeginInit();
                    bmp.UriSource = valueUri;
                    bmp.EndInit();
                    return bmp;
                default:
                    return new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}