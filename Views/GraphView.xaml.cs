using DevExpress.Mvvm;
using OxyTest.Composition;
using OxyTest.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace OxyTest.Views
{
	/// <summary>
	/// Interaction logic for GraphView.xaml
	/// </summary>
	public partial class GraphView : UserControl
	{
		public GraphView()
		{
			InitializeComponent();
			var core = new GraphCore();
			DataContext = new GraphViewModel(core);

			Initialize_GraphPlotArea(core);


			Loaded += (s, e) =>
			{
				//============================== parent winform의 handle을 가져오고, core에 세팅.
				var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
				if(hwndSource != null)
				{
					var elementHost = System.Windows.Forms.Control.FromChildHandle(hwndSource.Handle);
					var winformHandle = elementHost?.FindForm()?.Handle ?? IntPtr.Zero;
					core.InitializeWithHandle(winformHandle);
				}
			};
		}

		private void Initialize_GraphPlotArea(GraphCore core)
		{
			GraphPlotArea.Children.Add(core.NavigationService.NavigationFrame);
			core.NavigationService.Navigate(ePAGE_TYPE.SINGLE_Y);
		}

	}
}
