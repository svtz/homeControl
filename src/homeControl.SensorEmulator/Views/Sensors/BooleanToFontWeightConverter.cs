using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace homeControl.SensorEmulator.Views.Sensors
{
    [ValueConversion(typeof(bool), typeof(FontWeight))]
    class BooleanToFontWeightConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ArgumentException("value");
            }
            if (targetType != typeof(FontWeight))
            {
                throw new InvalidOperationException("Поддерживается конвертирование только в FontWeight");
            }

            return (bool)value ^ Inverse
                ? FontWeights.Bold
                : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Поддерживается только конвертация в прямом направлении.");
        }
    }
}
