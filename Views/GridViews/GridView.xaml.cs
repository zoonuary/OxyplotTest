using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using OxyTest.Models.Graph;
using OxyTest.ViewModels;
using OxyTest.ViewModels.GridViews.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
		private PopupMenu GridContextMenu { get; set; }
		private Popup ColumnChooser { get; set; }
		private GridPopupHelper GridPopupHelper { get; set; }

		public GridView(ResourceDictionary dictionary)
		{
			Resources.MergedDictionaries.Add(dictionary);
			InitializeComponent();

			this.DataContextChanged += (s, e) =>
			{
				if(DataContext is GridViewModel vm)
                {
					vm.GridControl = GridControl;
                }
			};

			Loaded += (s, e) =>
			{
				if (this.DataContext is GridViewModel viewmodel)
				{
					GridContextMenu = new PopupMenu();
					GridPopupHelper = viewmodel.GridPopupHelper;
					ColumnChooser = GridPopupHelper.CreateColumnChooserPopup(GridControl, viewmodel);
					
					BarManager.SetDXContextMenu(GridControl, GridContextMenu);
					GridContextMenu.Opening += GridControl_PopupMenuOpening;
				}
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

		private void GridControl_PopupMenuOpening(object sender, CancelEventArgs e)
        {
			var view = GridControl.View as TableView;
			var hitInfo = view?.CalcHitInfo(Mouse.GetPosition(view));

			GraphModel row = null;
			if (hitInfo?.RowHandle >= 0)
				row = GridControl.GetRow(hitInfo.RowHandle) as GraphModel;

			if(DataContext is GridViewModel viewmodel)
            {
				//row == null => 선택된 signal 없음
				GridContextMenu.Items.Clear();
				foreach (var item in GridPopupHelper.CreateGridControlPopupItems(row))
				{
					GridContextMenu.Items.Add((IBarItem)item);
				}
				GridContextMenu.Items.Add(GridPopupHelper.CreateColumnChooserButtonItem(ColumnChooser));
			}
		}
	}
}
