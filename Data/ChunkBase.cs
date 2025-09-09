using DevExpress.Drawing.Compatibility.Internal;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using OxyTest.Models.Graph.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public abstract class ChunkBase<T>:IChunk<T>
    {
		#region Properties
		public abstract SignalDataModel SignalDataModel { get; }
		public abstract double StartTime { get; }
		public abstract double EndTime { get; }
		#endregion

		#region Methods
		public virtual bool TryAppend(EventModel model) => false;
		public virtual BufferLease<T> SliceByTime(eVALUE_TYPE valueType, double start, double end) => BufferLease<T>.Empty;

		//public virtual SliceView<T> SliceByTIme(eVALUE_TYPE valueType, double start, double end) => SliceView<T>.Empty;
		public virtual bool SliceByTIme(eVALUE_TYPE valueType, double start, double end, Action<SliceView<GraphDataPoint>> action) => false;

		public int CalcRawValue(byte[] data, int startbit, int length, bool isUnsigned)
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

		public double CalcPhysicalValue(int rawvalue, double factor, double offset)
		{
			return (rawvalue * factor) + offset;
		}

		
		#endregion
	}
}
