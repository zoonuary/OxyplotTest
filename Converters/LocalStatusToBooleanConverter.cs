using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
    public class LocalStatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is eLOCAL_STATUS status
                && status == eLOCAL_STATUS.LIVEUPDATE)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b
                && b)
            {
                return eLOCAL_STATUS.LIVEUPDATE;
            }

            return eLOCAL_STATUS.PAUSED;
        }
    }
}
