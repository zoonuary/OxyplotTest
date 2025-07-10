using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Xpf.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace OxyTest.Views
{
	/// <summary>
	/// Interaction logic for GridView.xaml
	/// </summary>
	public partial class GridView : UserControl
	{
		public GridView(ResourceDictionary dictionary)
		{
			Resources.MergedDictionaries.Add(dictionary);
			InitializeComponent();

			this.DataContextChanged += (s, e) =>
			{

			};

		}

		//체크상태 즉시 적용을 위한 함수
		private void CheckBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(sender is CheckBox cbx)
			{
				cbx.IsChecked = !cbx.IsChecked;
				e.Handled = true;
			}
		}

		//combobox 내용 변경 시 바로 해당 내용 전달.
		private void ComboBoxEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
		{
			var combobox = sender as ComboBoxEdit;
			var bindingExpression = combobox.GetBindingExpression(ComboBoxEdit.EditValueProperty);
			bindingExpression?.UpdateSource();
		}
	}
}
