using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
    public class CursorTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            try
            {
                object enumValue = Enum.Parse(value.GetType(), parameter.ToString(), ignoreCase: true);
                return value.Equals(enumValue);
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b == true)
            {
                switch (parameter)
                {
                    case "DEFAULT":
                        return eCURSOR_TYPE.DEFAULT;

                    case "DIFFERENCE":
                        return eCURSOR_TYPE.DIFFERENCE;

                    case "MEASUERMENT":
                        return eCURSOR_TYPE.MEASUERMENT;
                }
            }
            return Binding.DoNothing;
        }
    }
}
