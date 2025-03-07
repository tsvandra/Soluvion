using System.Globalization;

namespace Soluvion.Converters
{
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string valueStr = value.ToString();
            string paramStr = parameter.ToString();

            string[] options = paramStr.Split(',');
            return options.Contains(valueStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}