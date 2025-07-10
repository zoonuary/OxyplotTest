using OxyTest.ViewModels;
using OxyTest.ViewModels.GridVIews.Internals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OxyTest.Converters
{
	public class ColumnVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if(values[0] is string fieldname
				&& values[1] is ObservableCollection<ColumnSetting> ColumnSettings)
			{
				var setting = ColumnSettings.FirstOrDefault(x => x.FieldName == fieldname);
				return setting?.IsVisible ?? true;
			}
			return true;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
