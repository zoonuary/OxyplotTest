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
			SignalDataModel = signalDataModel;
			GraphRenderModel = new GraphRenderModel(Tag, SignalDataModel);
			ValueType = eVALUE_TYPE.PHYSICAL;
			GraphRenderModel.Initialize();
			clearCursorValues();
		}

		#region Properties

		public object Tag { get; } = Guid.NewGuid();//Graph 식별용 Tag
		public SignalDataModel SignalDataModel { get; } //signal 정보를 담는 클래스
		public GraphRenderModel GraphRenderModel { get; }//실제로 그려질 부분에 대한 데이터나, 그리기 위해 필요한 동작 및 변수를 포함한 클래스

		private readonly object _listLock = new();
		//graph data를 공용으로 보관할 list
		private readonly List<GraphDataPoint> rawDataSource = new();
		private readonly List<GraphDataPoint> physicalDataSource = new();

		private bool selected;
		public bool Selected
		{
			get => selected;
			set
			{
				if (selected != value)
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

		private double? y;
		public double? Y
        {
			get => y;
            set
            {
				if(y != value)
                {
					y = value;
					NotifyPropertyChanged(nameof(Y));
                }
            }
        }

		private double? dy;
		public double? dY
        {
			get => dy;
            set
            {
				if(dy != value)
                {
					dy = value;
					NotifyPropertyChanged(nameof(dY));
                }
            }
        }

		private double? ytime;
		public double? YTime
		{
			get => ytime;
            set
            {
				if(ytime != value)
                {
					ytime = value;
					NotifyPropertyChanged(nameof(YTime));
                }
            }
        }

		private double? dytime;
		public double? dYTime
        {
			get => dytime;
            set
            {
				if(dytime != value)
                {
					dytime = value;
					NotifyPropertyChanged(nameof(dYTime));
                }
            }
        }
		#endregion

		#region Methods

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

		public void ClearData()
		{
			if (SignalDataModel == null) return;
			lock (_listLock)
			{
				rawDataSource.Clear();
				physicalDataSource.Clear();
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

		public double? GetNearestValue(double x)
		{
			double? resultValue = null;
			switch (ValueType)
			{
				case eVALUE_TYPE.RAW:
					resultValue = GetNearest_RawValue(x);
					break;
				case eVALUE_TYPE.PHYSICAL:
					resultValue = GetNearest_PhysicalValue(x);
					break;
			}
			return resultValue;
		}

		public void clearCursorValues()
        {
			Y = null;
			dY = null;
			YTime = null;
			dYTime = null;
        }
		#endregion

		#region SubMethods
		private int CalcRawValue(byte[] data, int startbit, int length, bool isUnsigned)
		{
			int startbyte = startbit / 8;
			int bitoffset = startbit % 8;
			int signalvalue = 0;
			int currentbyteindex, currentbitindex;

			for (int i = 0; i < length; i++)
			{
				currentbyteindex = startbyte + (bitoffset + i) / 8;
				currentbitindex = (bitoffset + i) % 8;

				if (data.Length <= currentbyteindex) break;

				int bitvalue = (data[currentbyteindex] >> currentbitindex) & 1;
				signalvalue |= bitvalue << i;
			}

			if (!isUnsigned && ((signalvalue >> (length - 1)) & 1) == 1)
			{
				signalvalue -= (1 << length);
			}

			return signalvalue;
		}

		private double CalcPhysicalValue(int rawvalue, double factor, double offset)
		{
			return (rawvalue * factor) + offset;
		}

		private void PushUpdatesToRenderModel(double minTime, double maxTime)
		{
			List<GraphDataPoint> snapshot;

			if (rawDataSource.Count > 0 && minTime < rawDataSource[0].X && minTime >= 0)
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

		private double? GetNearest_RawValue(double xValue)
		{
			double? resultvalue = null;

			lock (_listLock)
			{
				foreach (var point in rawDataSource)
				{
					if (point.X <= xValue)
					{
						resultvalue = point.Y;
					}
					else
					{
						break;
					}
				}
			}
			return resultvalue;
		}
		private double? GetNearest_PhysicalValue(double xValue)
		{
			double? resultValue = null;
			lock (_listLock)
			{
				foreach (var point in physicalDataSource)
				{
					if (point.X <= xValue)
					{
						resultValue = point.Y;
					}
					else
					{
						break;
					}
				}
			}
			return resultValue;
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
