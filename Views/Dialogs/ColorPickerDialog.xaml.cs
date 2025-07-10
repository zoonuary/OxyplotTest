using DevExpress.Xpf.Core;
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
	/// Interaction logic for ColorPickerDialog.xaml
	/// </summary>
	public partial class ColorPickerDialog : ThemedWindow
	{
		public Color SelectedColor { get; private set; }

		public ColorPickerDialog(Color currentColor)
		{
			InitializeComponent();
			ColorEditControl.EditValue = currentColor;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if(ColorEditControl.EditValue is Color color)
			{
				SelectedColor = color;
				DialogResult = true;
			}
			else
			{
				DialogResult = false;
			}
		}
	}
}