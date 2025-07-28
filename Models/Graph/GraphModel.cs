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
		public GraphModel() { }
		public GraphModel(SignalDataModel signalDataModel)
		{
			Tag = Guid.NewGuid();
			SignalDataModel = signalDataModel;
			GraphRenderModel = new GraphRenderModel(Tag, SignalDataModel);
			ValueType = eVALUE_TYPE.PHYSICAL;
			GraphRenderModel.Initialize();
		}

		public object Tag { get; }

		//model이 생성될 때, 들어오는 signal 정보 클래스
		public SignalDataModel SignalDataModel { get; }
		//실제로 그려질 부분만 담아두기 용이하도록 만든 Render Model 클래스
		public GraphRenderModel GraphRenderModel { get; }

		private bool selected;
		public bool Selected
        {
			get => selected;
            set
            {
				if(selected != value)
                {
					selected = value;
					GraphRenderModel.Selected = value;
					//NotifyPropertyChanged(nameof(Selected));
				}
            }
        }


		private eVALUE_TYPE valueType;
		public eVALUE_TYPE ValueType
		{
			get => valueType;
			set
			{
				if (valueType != value)
				{
					valueType = value;
					GraphRenderModel.ValueType = value;
					NotifyPropertyChanged(nameof(ValueType));
				}
			}
		}

		private readonly object _listLock = new();
		//graph data를 공용으로 보관할 list
		private readonly List<GraphDataPoint> rawDataSource = new();

		private readonly List<GraphDataPoint> physicalDataSource = new();

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

		private void PushUpdatesToRenderModel(double minTime, double maxTime)
		{
			List<GraphDataPoint> snapshot;
			
			if(rawDataSource.Count > 0 && minTime < rawDataSource[0].X && minTime >= 0)
            {
				//Todo : 중간에 생성된 그래프는 다시 로드할 수 있도록 방편 마련해야함
				//Todo : 추가로, 시간이 길어질 수록 앞 데이터는 buffer에서도 사라짐으로, 가지고있는 리스트의 시간보다 작은 시간을 원하는지 확인해서 가져와야함.
			}


			switch (ValueType)
			{
				case eVALUE_TYPE.RAW:
					lock (_listLock)
					{
						snapshot = rawDataSource.Where(datapoint => minTime < datapoint.X && datapoint.X < maxTime).ToList();
					}
					GraphRenderModel.UpdateSeries(snapshot);
					break;
				case eVALUE_TYPE.PHYSICAL:
					lock (_listLock)
					{
						snapshot = physicalDataSource.Where(datapoint => minTime < datapoint.X && datapoint.X < maxTime).ToList();
					}
					GraphRenderModel.UpdateSeries(snapshot);
					break;
			}
		}

		public void UpdatePlotData(Axis sender) //value type 에 따라 달라진 값 푸시
        {
			double defaultOffset = 2.0;//확대했을때 좌우 끊김 현상을 방지하기위해 좌우로 좀 더 가져옴
			PushUpdatesToRenderModel(sender.ActualMinimum - defaultOffset, sender.ActualMaximum + defaultOffset);
        }

		public void UpdatePlotData(double minTime, double maxTime)
        {
			PushUpdatesToRenderModel(minTime, maxTime);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
