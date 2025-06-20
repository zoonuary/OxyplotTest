using DevExpress.Xpf.Core;
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
	/// Interaction logic for SignalAddConfirmDialog.xaml
	/// </summary>
	public partial class SignalAddConfirmDialog : ThemedWindow
	{
		public SignalAddConfirmDialog(List<string> signalName)
		{
			InitializeComponent();
			DataContext = new SignalAddConfirmDialogViewModel(signalName, this);
			btnOK.Focus();
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
