using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public class StopChunk : ChunkBase<GraphDataPoint>
    {
        public StopChunk(double startTime, double endTime)
        {
            HasData = false;
            StartTime = startTime;
            EndTime = EndTime;
        }

        public StopChunk(SignalDataModel signalDataModel, double startTime, double endTime, EventModel[] data)
        {
            SignalDataModel = signalDataModel;
            StartTime = startTime;
            EndTime = endTime;
            SetPoints(data);
        }

        #region Properties
        public override SignalDataModel SignalDataModel { get; }
        public override double StartTime { get; }
        public override double EndTime { get; }

        public bool HasData { get; private set; } = false;

        private GraphDataPoint[] RawPoints;
        private GraphDataPoint[] PhysicalPoints;
        #endregion

        #region Methods

        public override BufferLease<GraphDataPoint> SliceByTime(eVALUE_TYPE valueType, double start, double end)
        {
            if (end < StartTime || start > EndTime || start > end)
                return BufferLease<GraphDataPoint>.Empty;

            GraphDataPoint[] DataArray;
            if (valueType == eVALUE_TYPE.RAW)
                DataArray = RawPoints;
            else
                DataArray = PhysicalPoints;

            if (DataArray == null || DataArray.Length <= 0)
                return BufferLease<GraphDataPoint>.Empty;

            int startIdx = LowerBound(DataArray, start);
            int endIdx = UpperBound(DataArray, end);
            int length = endIdx - startIdx;

            if (length <= 0)
                return BufferLease<GraphDataPoint>.Empty;

            var lease = BufferLease<GraphDataPoint>.FromPooled(length);
            Array.Copy(DataArray, startIdx, lease.Data, 0, length);

            return lease;
        }

        public SliceResult<GraphDataPoint> GetSegment(eVALUE_TYPE valueType, double start, double end)
        {
            var DataArray = valueType == eVALUE_TYPE.RAW ? RawPoints : PhysicalPoints;
            if(DataArray == null || DataArray.Length <= 0)
            {
                return SliceResult<GraphDataPoint>.Empty;
            }

            if(double.IsNaN(start) || double.IsNaN(end) || start > end)
            {
                return SliceResult<GraphDataPoint>.Empty;
            }

            int startIdx = LowerBound(DataArray, start);
            int endIdx = UpperBound(DataArray, end);
            int length = endIdx - startIdx;

            bool hasLeft = startIdx > 0;
            var left = hasLeft ? DataArray[startIdx - 1] : default;

            bool hasRight = endIdx < DataArray.Count();
            var right = hasRight ? DataArray[endIdx] : default;

            var resultSeg =
                length > 0
                ? new SliceSegment<GraphDataPoint>(DataArray, startIdx, length)
                : SliceSegment<GraphDataPoint>.Empty;

            return new SliceResult<GraphDataPoint>(resultSeg, left, hasLeft, right, hasRight);
        }

        public GraphDataPoint? GetPoint(eVALUE_TYPE valueType, int index)
        {
            var dataArray = valueType == eVALUE_TYPE.RAW ? RawPoints : PhysicalPoints;
            if (dataArray == null || dataArray.Length <= 0)
                return null;
            return dataArray[index];
        }

        public GraphDataPoint? GetLast(eVALUE_TYPE valueType)
        {
            var dataArray = valueType == eVALUE_TYPE.RAW ? RawPoints : PhysicalPoints;
            if (dataArray == null || dataArray.Length <= 0)
                return null;
            return dataArray[dataArray.Count() - 1];
        }

        public GraphDataPoint? GetFirst(eVALUE_TYPE valueType)
        {
            var dataArray = valueType == eVALUE_TYPE.RAW ? RawPoints : PhysicalPoints;
            if (dataArray == null || dataArray.Length <= 0)
                return null;
            return dataArray[0];
        }

        public double? GetNearest_Raw(double x)
        {
            var nearestIdx = UpperBound(RawPoints, x) - 1; // upperbound : upperbound > Point.X 인 첫 번째 인덱스.
            if (nearestIdx < 0) //list에 x <= 인 RawPoints.X가 없음.
            {
                return null;
            }
            return RawPoints[nearestIdx].Y;
        }

        public double? GetNearest_Phys(double x)
        {
            var nearestIdx = UpperBound(PhysicalPoints, x) - 1;
            if(nearestIdx < 0)
            {
                return null;
            }
            return PhysicalPoints[nearestIdx].Y;
        }

        #endregion

        #region SubMethods
        private void SetPoints(EventModel[] src)
        {
            int count = 0;
            foreach(var model in src)
            {
                if (model != null && model.ID == SignalDataModel.ID) count++;
            }

            if(count <= 0)
            {
                HasData = false;
                return;
            }
            else
            {
                HasData = true;
            }

            RawPoints = new GraphDataPoint[count];
            PhysicalPoints = new GraphDataPoint[count];

            int j = 0;
            for (int i = 0; i < src.Count(); i++)
            {
                if (src[i] == null || src[i].ID != SignalDataModel.ID) continue;
                int rawValue = CalcRawValue(src[i].Data, SignalDataModel.StartBit, SignalDataModel.Length, SignalDataModel.IsUnsigned);
                double physicalValue = CalcPhysicalValue(rawValue, SignalDataModel.Factor, SignalDataModel.Offset);

                RawPoints[j] = new GraphDataPoint(src[i].Index, src[i].TimeStamp, rawValue);
                PhysicalPoints[j] = new GraphDataPoint(src[i].Index, src[i].TimeStamp, physicalValue);
                j++;
            }
        }

        private int LowerBound(GraphDataPoint[] src, double target)
        {
            int low = 0;
            int high = src.Length;
            while (low < high)
            {
                int middle = (low + high - 1) >> 1;
                if (src[middle].X < target)
                    low = middle + 1;
                else
                    high = middle;
            }
            return low < src.Length ? low : src.Length;
        }

        private int UpperBound(GraphDataPoint[] src, double target)
        {
            int low = 0;
            int high = src.Length;
            while (low < high)
            {
                int middle = (low + high - 1) >> 1;
                if (src[middle].X <= target)
                    low = middle + 1;
                else
                    high = middle;
            }
            return low;
        }
        #endregion
    }
}
