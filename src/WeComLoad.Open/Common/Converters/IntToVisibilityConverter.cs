using System.Globalization;
using System.Windows.Data;

namespace WeComLoad.Open.Common.Converters;

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vi = Visibility.Visible;
            if (value is int && (int)value > 0)
            {
                vi = Visibility.Hidden;
            }
            return vi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }

            return false;
        }
    }
