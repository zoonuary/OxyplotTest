using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
	/// <summary>
	/// GraphModel은 하나의 signal에 1:1 대응하도록 설계됨.
	/// </summary>
	public class GraphModel : INotifyPropertyChanged
	{
		public GraphModel(SignalDataModel signalDataModel)
		{
			SignalDataModel = signalDataModel;
			GraphRenderModel = new GraphRenderModel();
		}

		//model이 생성될 때, 들어오는 signal 정보 클래스
		public SignalDataModel SignalDataModel { get; }

		//실제로 그려질 부분만 담아두기 용이하도록 만든 Render Model 클래스
		public GraphRenderModel GraphRenderModel { get; }

		private readonly object _listLock = new object();
		//graph data를 공용으로 보관할 list
		private readonly List<GraphDataPoint> rawDataSource = new List<GraphDataPoint>();

		private readonly List<GraphDataPoint> physicalDataSource = new List<GraphDataPoint>();

		////datapoints
		//public Series DataSeries { get; set; } 
		//public DateTimeAxis XAxis { get; set; }
		//public LinearAxis YAxis { get; set; }

		//LineSeries series;
		//BarSeries seriesbar;
		//AreaSeries seriesarea;
		//ScatterSeries seriespoint;

		//PlotModel model;

		private int CalcRawValue(byte[] data, int startbit, int length, bool isUnsigned)
		{
			int startbyte = startbit / 8;
			int bitoffset = startbit % 8;
			int signalvalue = 0;
			int currentbyteindex, currentbitindex;

			for(int i = 0; i < length; i++)
			{
				currentbyteindex = startbyte + (bitoffset + i) / 8;
				currentbitindex = (bitoffset + i) % 8;

				if (data.Length <= currentbyteindex) break;

				int bitvalue = (data[currentbyteindex] >> currentbitindex) & 1;
				signalvalue |= bitvalue << i;
			}

			if(!isUnsigned && ((signalvalue >> (length - 1)) & 1) == 1)
			{
				signalvalue -= (1 << length);
			}

			return signalvalue;
		}

		private double CalcPhysicalValue(int rawvalue, double factor, double offset)
		{
			return (rawvalue * factor) + offset;
		}

		//바로 사용 가능하도록 model data에 추가. Datasource와 plot의 데이터 동기화 및 rendering은 ui thread에서 할 일.
		public void AppendData(EventModel eventModel)
		{
			if (SignalDataModel == null) return;
			
			int rawValue = CalcRawValue(eventModel.Data, SignalDataModel.StartBit, SignalDataModel.Length, SignalDataModel.IsUnsigned);
			double physicalValue = CalcPhysicalValue(rawValue, SignalDataModel.Factor, SignalDataModel.Offset);

			lock (_listLock)
			{
				rawDataSource.Add(new GraphDataPoint
				{
					X = eventModel.TimeStamp,
					Y = rawValue
				});

				physicalDataSource.Add(new GraphDataPoint
				{
					X = eventModel.TimeStamp,
					Y = physicalValue
				});
			}	
		}

		public void PushListToRenderModel()
		{
			List<GraphDataPoint> Snapshot;
			lock (_listLock) //Todo : raw / physical mode에 따른 분기처리 필요
			{
				//var rawDataSnapshot = rawDataSource.ToList();
				//var physicalDataSnapshot = physicalDataSource.ToList();
				Snapshot = physicalDataSource.ToList();
			}

			//todo : 보여지는 부분만 별도의 연산처리 필요
			GraphRenderModel.UpdateSeries(Snapshot);
		}


		//public IReadOnlyList<GraphDataPoint> RawDataSource => rawDataSource;
		//public IReadOnlyList<GraphDataPoint> PhysicalDataSource => physicalDataSource;

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
