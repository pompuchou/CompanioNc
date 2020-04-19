using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CompanioNc.ViewModels.Converters
{
    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                bool b;
                if (bool.TryParse((string)value, out b))
                {
                    if (b == true) return Brushes.Red;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}