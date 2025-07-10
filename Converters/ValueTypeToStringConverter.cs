using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
	public class ValueTypeToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is eVALUE_TYPE type)
			{
				switch (type)
				{
					case eVALUE_TYPE.RAW:
						return "Raw";
					case eVALUE_TYPE.PHYSICAL:
						return "Physical";
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
