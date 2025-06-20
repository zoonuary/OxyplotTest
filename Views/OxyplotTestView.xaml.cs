using DevExpress.Mvvm.CodeGenerators;
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
	/// Interaction logic for OxyplotTestView.xaml
	/// </summary>
	public partial class OxyplotTestView : UserControl
	{
		public OxyplotTestView()
		{
			InitializeComponent();
			DataContext = new OxyplotTestViewModel();
		}

		public OxyplotTestView(OxyplotTestViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
		}
		
	}
}
