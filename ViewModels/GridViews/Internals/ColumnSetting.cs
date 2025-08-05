using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.ViewModels.GridVIews.Internals
{
	public class ColumnSetting:INotifyPropertyChanged
	{
		/// <summary>
		/// GridControl columns기준 header값을 넣어둠
		/// </summary>
		public string Header { get; set; }
		private bool? isVisible;
		public bool? IsVisible
		{
			get => isVisible;
			set
			{
				if (isVisible != value)
				{
					isVisible = value;
					NotifyPropertyChanged(nameof(IsVisible));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
