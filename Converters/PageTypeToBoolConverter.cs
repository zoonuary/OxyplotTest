using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
    class PageTypeToBoolConverter : IValueConverter
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
                    case "SINGLE_Y":
                        return ePAGE_TYPE.SINGLE_Y;

                    case "MULTIPLE_Y":
                        return ePAGE_TYPE.MULTIPLE_Y;

                    case "SEPARATE_Y":
                        return ePAGE_TYPE.SEPARATE_Y;
                }
            }
            return Binding.DoNothing;
        }
    }
}
