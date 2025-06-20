using DevExpress.Mvvm;
using OxyTest.Composition;
using OxyTest.Data;

namespace OxyTest.ViewModels
{
	public class GridViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }
		public GraphData GraphData { get; }
		public GridViewModel(GraphCore graphCore)
		{
			GraphCore = graphCore;
			GraphData = GraphCore.GraphData;
		}
	}
}
