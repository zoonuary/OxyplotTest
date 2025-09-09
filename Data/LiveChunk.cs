using DevExpress.Drawing.Compatibility.Internal;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public class LiveChunk : ChunkBase<GraphDataPoint>
    {
        public LiveChunk(SignalDataModel signalDataModel, int capa)
        {
            SignalDataModel = signalDataModel;

            Capacity = capa;
            Head = 0;
            Tail = 0;
            Count = 0;
            Version = 0;
            RawPoints = new GraphDataPoint[Capacity];
            PhysicalPoints = new GraphDataPoint[Capacity];
        }

        #region Properties
        public override SignalDataModel SignalDataModel { get; }
        public override double StartTime => Count == 0 ? 0 : RawPoints[Head].X; //raw physical 뭐로 가져오든 상관은 없음
        public override double EndTime => Count == 0 ? 0 : RawPoints[(Tail - 1 + Capacity) % Capacity].X;

        private long Version;
        private GraphDataPoint[] RawPoints;
        private GraphDataPoint[] PhysicalPoints;
        private readonly int Capacity;
        private int Head, Tail;
        private int Count;
        #endregion

        #region Methods
        public void Clear()
        {
            Head = 0;
            Tail = 0;
            Count = 0;
            Version = 0;
            RawPoints = new GraphDataPoint[Capacity];
            PhysicalPoints = new GraphDataPoint[Capacity];
        }

        public sealed override bool TryAppend(EventModel model)
        {
            if (EndTime > model.TimeStamp) return false; //graph point 역행 방지 

            int next = (Tail + 1) % Capacity;
            if (next == Head) //buffer full
            {
                Head = (Head + 1) % Capacity;
            }
            else
            {
                Count++;
            }

            int rawValue = CalcRawValue(model.Data, SignalDataModel.StartBit, SignalDataModel.Length, SignalDataModel.IsUnsigned);
            double physicalValue = CalcPhysicalValue(rawValue, SignalDataModel.Factor, SignalDataModel.Offset);

            RawPoints[Tail] = new GraphDataPoint(model.Index, model.TimeStamp, rawValue);
            PhysicalPoints[Tail] = new GraphDataPoint(model.Index, model.TimeStamp, physicalValue);
            Tail = next;

            Interlocked.Increment(ref Version);
            return true;
        }

        public override BufferLease<GraphDataPoint> SliceByTime(eVALUE_TYPE valueType, double start, double end)
        {
            const int maxRetry = 1; //데이터가 흐름 중간에 변경 시, 재시도 횟수

            for (int attempt = 0; attempt <= maxRetry; attempt++)
            {
                long v0 = Volatile.Read(ref Version); //데이터 무결성 확인용 버전 카운터
                int head = Head;
                int count = Count;
                GraphDataPoint[] DataPoints;
                if (valueType == eVALUE_TYPE.RAW)
                    DataPoints = RawPoints;
                else
                    DataPoints = PhysicalPoints;

                if (count <= 0)
                    return BufferLease<GraphDataPoint>.Empty;

                int startIdx = LowerBound(DataPoints, start, head, count, Capacity); //rawpoints를 넣으나 phys를 넣으나 어차피 timestamp기반 idx임
                int endIdx = UpperBound(DataPoints, end, head, count, Capacity);
                int length = endIdx - startIdx;

                if (length <= 0)
                    return BufferLease<GraphDataPoint>.Empty;

                var lease = BufferLease<GraphDataPoint>.FromPooled(length);
                CopySlice(DataPoints, lease, 0, startIdx, length, head, Capacity);

                long v1 = Volatile.Read(ref Version);
                if (v0 == v1)
                {
                    return lease;
                }

                lease.Dispose();
                if (attempt == maxRetry)
                    return BufferLease<GraphDataPoint>.Empty;

                continue;
            }
            return BufferLease<GraphDataPoint>.Empty;
        }

        public override bool SliceByTIme(eVALUE_TYPE valueType, double start, double end, Action<SliceView<GraphDataPoint>> action)
        {
            const int maxRetry = 1; //데이터가 흐름 중간에 변경 시, 재시도 횟수

            for (int attempt = 0; attempt <= maxRetry; attempt++)
            {
                long v0 = Volatile.Read(ref Version); //데이터 무결성 확인용 버전 카운터
                int head = Head;
                int count = Count;

                GraphDataPoint[] DataPoints = (valueType == eVALUE_TYPE.RAW) ? RawPoints : PhysicalPoints;

                if (count <= 0)
                    return false;

                int startIdx = LowerBound(DataPoints, start, head, count, Capacity); //rawpoints를 넣으나 phys를 넣으나 어차피 timestamp기반 idx임
                int endIdx = UpperBound(DataPoints, end, head, count, Capacity);
                int length = endIdx - startIdx;

                if (length <= 0)
                    return false;

                // 논리 => 물리 변환
                int physStart = (head + startIdx) % Capacity;
                int firstLen = Math.Min(length, Capacity - physStart); // x >= len : ring size만큼 커지지 않음. firstLen = length. x < len : 충분히 커져서 오버됨, 분할 필요
                int secondLen = length - firstLen;

                var firstSeg = new SliceSegment<GraphDataPoint>(DataPoints, physStart, firstLen);
                var secondSeg = new SliceSegment<GraphDataPoint>(DataPoints, 0, secondLen);
                var view = new SliceView<GraphDataPoint>(firstSeg, secondSeg);

                action(view);

                long v1 = Volatile.Read(ref Version);
                if (v0 == v1)
                {
                    return true;
                }

                if (attempt == maxRetry)
                    return false;

                continue;
            }
            return false;
        }

        #endregion

        #region SubMethods
        private int LowerBound(GraphDataPoint[] datas, double target, int head, int count, int capacity)
        {
            int low = 0;
            int high = count;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                int phys = (head + mid) % capacity;
                if (datas[phys].X < target)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }

        private int UpperBound(GraphDataPoint[] datas, double target, int head, int count, int capacity)
        {
            int low = 0;
            int high = count;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                int phys = (head + mid) % capacity;
                if (datas[phys].X <= target)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }

        private void CopySlice(GraphDataPoint[] buffer, BufferLease<GraphDataPoint> dest, int destOffset, int logicalStart, int length, int head, int capacity)
        {
            if (length <= 0) return;

            int physStart = (head + logicalStart) % capacity;                   //논리 idx => 물리 idx로 변환
            int firstPart = Math.Min(length, capacity - physStart);             //변환된 첫 구간(인덱스 [physStart] ~ [capacity - 1])
            dest.AppendRange(buffer, physStart, firstPart);

            int remain = length - firstPart;
            if (remain > 0)
                dest.AppendRange(buffer, 0, remain);
        }
        #endregion
    }
}
