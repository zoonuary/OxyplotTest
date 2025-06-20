using DevExpress.Mvvm;
using OxyPlot;
using OxyTest.Composition;

namespace OxyTest.ViewModels
{
	public class SingleYPlotViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }

		public SingleYPlotViewModel(GraphCore graphCore)
		{
			GraphCore = graphCore;
			PlotModel = new PlotModel();
			//Graph의 dataModel을 update하는 트리거 등록
			GraphCore.SubscribeModelUpdates(UpdatePlotModel);
		}

		public PlotModel PlotModel { get; }

		private void UpdatePlotModel()
		{
			PlotModel.Series.Clear();
			foreach (var graphModel in GraphCore.GraphData.Graphs)
			{
				PlotModel.Series.Add(graphModel.GraphRenderModel.CurrentSeries);
			}
			PlotModel.InvalidatePlot(true);
		}
	}
}
