using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using OxyTest.ViewModels.Dialogs;
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
using System.Windows.Shapes;


namespace OxyTest.Views.Dialogs
{
	/// <summary>
	/// Interaction logic for SignalAddDialog.xaml
	/// </summary>
	public partial class SignalAddDialog : ThemedWindow
	{
		//public SignalAddDialog()
		//{
		//	InitializeComponent();
		//}

		public SignalAddDialog(Action<NodeItem> OnSignalAdded)
		{
			InitializeComponent();
			DataContext = new SignalAddDialogViewModel(OnSignalAdded, this);
		}

		private void TreeList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TreeListControl treeControl)
			{
				var view = treeControl.View as TreeListView;
				var node = view?.FocusedNode;

				if (node != null)
				{
					view.ExpandNode(node.RowHandle);
				}
			}
		}

		private void TreeList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TreeListControl treeControl)
			{
				var view = treeControl.View as TreeListView;
				var node = view?.FocusedNode;
				if (node != null)
				{
					if (node.IsExpanded)
					{
						view.CollapseNode(node.RowHandle);
					}
				}
			}
		}

		private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogResult = false;
			}
		}
	}
}
