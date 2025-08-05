using DevExpress.Mvvm;
using OxyTest.Composition;
using OxyTest.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private static readonly ResourceDictionary dictionary = new ResourceDictionary
		{
			Source = new Uri("pack://application:,,,/OxyTest;component/Themes/Converters.xaml")
		};

		public GraphView()
		{
			var core = new GraphCore(this); //모듈에 필요한 주요 클래스 전부 로드
			Resources.MergedDictionaries.Add(dictionary); //resource dictionary 추가.
			InitializeComponent();

			GridViewModel gridViewModel = Initialize_GridViewContainer(core);
			Initialize_GraphPlotContainer(core);

			DataContext = new GraphViewModel(core, gridViewModel);
		}

		//Graph plot 내부 NavigationFrame 초기화 및 초기 화면(SINGLE_Y)으로 로드
		private void Initialize_GraphPlotContainer(GraphCore core)
		{
			var plotview = new PlotView()
			{
				DataContext = new PlotViewModel(core)
			};
			GraphPlotContainer.Children.Add(plotview);
		}

		//Gridview 생성 및 초기화
		private GridViewModel Initialize_GridViewContainer(GraphCore graphCore)
		{
			var viewmodel = new GridViewModel(graphCore);
			var gridview = new GridView(dictionary)
			{
				DataContext = viewmodel
			};
			GridViewContainer.Children.Add(gridview);
			return viewmodel;
		}

        private void PopupMenu_Closed(object sender, EventArgs e)
        {
			LabelSplitCheckItem.IsChecked = false;
			ViewTypeSplitCheckItem.IsChecked = false;
		}
    }
}
