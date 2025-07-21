using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
    class PlotModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ePLOT_MODE type)
            {
                switch (type)
                {
                    case ePLOT_MODE.LINE:
                        return "Line";
                    case ePLOT_MODE.POINT:
                        return "Point";
                    case ePLOT_MODE.LINE_POINT:
                        return "Line + Point";
                }
            }
            return "UNKNOWN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
