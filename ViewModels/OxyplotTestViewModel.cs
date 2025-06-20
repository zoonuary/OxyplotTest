using DevExpress.Mvvm.CodeGenerators;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;

namespace OxyTest.Views
{
	public partial class OxyplotTestViewModel
	{

		public PlotModel plotModel { get; set; }

		public OxyplotTestViewModel()
		{
			plotModel = new PlotModel();
			var axis = new DateTimeAxis
			{
				Position = AxisPosition.Bottom,
				StringFormat = "HH:mm:ss.fff",
			};
			plotModel.Axes.Add(axis);

			var yaxis1 = new LinearAxis
			{
				Position = AxisPosition.Left,
				Key = "Y1",
				PositionTier = 0,
				LabelFormatter = val =>
				{
					string returnValue = string.Empty;
					switch (val)
					{
						case 0:
							returnValue = "영";
							break;
						case 5:
							returnValue = "오";
							break;
						case 15:
							returnValue = "십오";
							break;
						default:
							returnValue = val.ToString();
							break;
					}
					return returnValue;
				}
			};

			plotModel.Axes.Add(yaxis1);

			var yaxis2 = new LinearAxis
			{
				Position = AxisPosition.Left,
				Key = "Y2",
				PositionTier = 1,
				StartPosition = 0.5,
				EndPosition = 1.0
			};
			plotModel.Axes.Add(yaxis2);

			var time = DateTime.MinValue;


			var series1 = new LineSeries();
			series1.YAxisKey = "Y1";

			//series1.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(1), 0));
			//series1.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(3), 15));
			//series1.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(5), 2));

			var series2 = new LineSeries();
			series2.YAxisKey = "Y2";

			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time, 11));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(2), 2));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(4), 8));

			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(6), 2));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(8), 8));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(10), 2));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(12), 8));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(14), 2));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(16), 8));
			//series2.Points.Add(DateTimeAxis.CreateDataPoint(time.AddSeconds(18), 2));

			var scatterseries = new ScatterSeries();
			
			

			var areaseries = new AreaSeries();


			var barseries = new BarSeries();

			//scatterseries.Points.Add(new ScatterPoint())



			//barseries.Items.Add(new baritem);

			plotModel.Series.Add(series1);
			plotModel.Series.Add(series2);

		}
	}
}
