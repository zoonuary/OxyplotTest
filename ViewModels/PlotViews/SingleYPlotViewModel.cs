using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyTest.Composition;
using System;

namespace OxyTest.ViewModels
{
	public class SingleYPlotViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }

		public SingleYPlotViewModel(GraphCore graphCore)
		{
			//
			GraphCore = graphCore;
			
			//plot model
			PlotModel = new PlotModel();
			SetDefaultAxis(PlotModel);

			//Graph의 dataModel을 update하는 트리거 등록
			GraphCore.SubscribeModelUpdates(UpdatePlotModel);

			//
			graphCore.GraphData.Graphs_CollectionChanged += OnGraphCollectionChanged;
			graphCore.GraphData.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(graphCore.GraphData.SelectedModel): //선택된 item(model) 변경 시 알림
						break;
				}
			};
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

		private void OnGraphCollectionChanged(object sender, EventArgs e)
		{
			PlotModel.Axes.Clear();
			foreach(var graph in GraphCore.GraphData.Graphs)
			{
				PlotModel.Axes.Add(graph.GraphRenderModel.YAxis);
			}
			SetDefaultAxis(PlotModel);
		}

		private void SetDefaultAxis(PlotModel model)
		{//datetime 으로 표현될 이유가 부족해서 중단.
		 // 혹시 time format이 HH:mm:ss.fff로 표현되어야한다면 stringformat 대신 labelformat을 찾아볼 것.

			var XAxis = new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Time (s)",
				//LabelFormatter = value =>
				//{
				//	return $"{value:0.0}s";
				//}

				MajorGridlineStyle = LineStyle.Solid,
				MinorGridlineStyle = LineStyle.Dot,
				MajorGridlineColor = OxyColors.Gray,
				MinorGridlineColor = OxyColors.LightGray
			};

			model.Axes.Add(XAxis);
		}
	}
}
