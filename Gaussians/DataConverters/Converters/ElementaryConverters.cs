using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gaussians.DataConverters
{
    internal class DoubleConverter : IValidationConverter
    {
        public bool CanValidation(object obj, CultureInfo cultureInfo)
        {
            double temp = new();
            return double.TryParse(obj.ToString(), out temp);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value);
        }
    }
    internal class IntConverter : IValidationConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value);
        }

        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            int temp = new();
            return int.TryParse(value.ToString(), out temp);
        }
    }
    internal class StringConverter : IValidationConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            return true;
        }
    }

    internal class BoolConverter : IValidationConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value);
        }

        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            bool temp = new();
            return bool.TryParse(value.ToString(), out temp);
        }
    }

    internal class ObjectConverter : IValidationConverter
    {
        public bool CanValidation(object value, CultureInfo cultureInfo)
        {
            return value != null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
