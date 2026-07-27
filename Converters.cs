using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BarterPOS
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = value is bool b && b;
            return isActive
                ? new SolidColorBrush(Color.FromRgb(0x05, 0x96, 0x69))
                : new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ActiveToButtonLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = value is bool b && b;
            return isActive ? "Deactivate" : "Activate";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ActiveToActionBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = value is bool b && b;
            return isActive
                ? new SolidColorBrush(Color.FromRgb(0xEA, 0x58, 0x0C))
                : new SolidColorBrush(Color.FromRgb(0x1E, 0x40, 0xAF));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount.ToString("N2", culture);
            }

            if (value is double d)
            {
                return d.ToString("N2", culture);
            }

            return "0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
