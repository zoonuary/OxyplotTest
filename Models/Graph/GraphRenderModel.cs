using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
	public class GraphRenderModel : INotifyPropertyChanged
	{
		/*
		 * GraphRenderModel에선 화면에 그려질 때만 필요한 데이터들을 취급
		 */
		public GraphRenderModel()
		{
			Initialize();
		}

		private void Initialize()
		{
			PlotMode = ePLOT_MODE.LINE;
		}

		public ePLOT_MODE PlotMode; //Todo : PlotMode가 바뀌는 경우, 바뀌게 되는 다른 series에 데이터를 추가해야함, UpdateSeries를 활용하는게 좋아보임

		public Series CurrentSeries
		{
			get
			{
				switch (PlotMode)
				{
					case ePLOT_MODE.LINE:
						return lineSeries;
					case ePLOT_MODE.BAR:
						return barSeries;
					case ePLOT_MODE.AREA:
						return areaSeries;
					case ePLOT_MODE.POINT:
						return scatterSeries;
				}
				return null;
			}
		}

		private LineSeries lineSeries = new LineSeries();
		private AreaSeries areaSeries = new AreaSeries();
		private BarSeries barSeries = new BarSeries();
		private ScatterSeries scatterSeries = new ScatterSeries();

		

		//graphdatapoint list 전부 clear, parameter인 list<graphdatapoint>로 새로 데이터 주입
		public void UpdateSeries(IEnumerable<GraphDataPoint> data)
		{
			switch (PlotMode)
			{
				case ePLOT_MODE.LINE:
					UpdateLineSeries(data);
					break;
				case ePLOT_MODE.BAR:
					//UpdateBarSeires(data);
					break;
				case ePLOT_MODE.AREA:
					UpdateAreaSeries(data);
					break;
				case ePLOT_MODE.POINT:
					UpdatePointSeries(data);
					break;
			}
		}

		private void UpdateLineSeries(IEnumerable<GraphDataPoint> data)
		{
			lineSeries.Points.Clear();
			foreach(GraphDataPoint point in data)
			{
				lineSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.X, point.Y));
			}
		}

		//bar 는 index 기반이기에 구현에 장애가 많음.
		//private void UpdateBarSeires(IEnumerable<GraphDataPoint> data)
		//{
		//	barSeries.Items.Clear();
		//	foreach(GraphDataPoint point in data)
		//	{
		//		barSeries.Items.Add()
		//	}
		//}

		private void UpdateAreaSeries(IEnumerable<GraphDataPoint> data)
		{
			areaSeries.Points.Clear();
			areaSeries.Points2.Clear();

			foreach(GraphDataPoint point in data)
			{
				areaSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.X, point.Y));
			}
		}

		private void UpdatePointSeries(IEnumerable<GraphDataPoint> data)
		{
			scatterSeries.Points.Clear();
			foreach(GraphDataPoint point in data)
			{
				scatterSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(point.X), point.Y));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
