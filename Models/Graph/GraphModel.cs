using OxyPlot.Axes;
using OxyTest.Data;
using OxyTest.Models.Event;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace OxyTest.Models.Graph
{
    /// <summary>
    /// GraphModel은 하나의 signal에 1:1 대응하도록 설계됨.
    /// </summary>
    public class GraphModel : INotifyPropertyChanged
    {
        public GraphModel() { }
        public GraphModel(SignalDataModel signalDataModel, Color color)
        {
            SignalDataModel = signalDataModel;
            GraphRenderModel = new GraphRenderModel(Tag, SignalDataModel);
            ValueType = eVALUE_TYPE.PHYSICAL;
            GraphRenderModel.Initialize(color);
            clearCursorValues();

            LiveChunk = new LiveChunk(SignalDataModel, 10000);
        }

        #region Properties

        public object Tag { get; } = Guid.NewGuid();//Graph 식별용 Tag
        public SignalDataModel SignalDataModel { get; } //signal 정보를 담는 클래스
        public GraphRenderModel GraphRenderModel { get; }//실제로 그려질 부분에 대한 데이터나, 그리기 위해 필요한 동작 및 변수를 포함한 클래스

        private readonly object _listLock = new();
        //graph data를 공용으로 보관할 list
        private readonly List<GraphDataPoint> rawDataSource = new();
        private readonly List<GraphDataPoint> physicalDataSource = new();

        private LiveChunk LiveChunk { get; }
        private StopChunk[] StopChunks = new StopChunk[3];

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
                if (y != value)
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
                if (dy != value)
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
                if (ytime != value)
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
                if (dytime != value)
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
            if (SignalDataModel == null)
            {
                return;
            }

            LiveChunk.TryAppend(eventModel);
        }

        public void ClearData()
        {
            LiveChunk.Clear();
            StopChunks = new StopChunk[3];

            //if (SignalDataModel == null) return;
            //while (RawDataTail.TryDequeue(out _)) ;
            //while (PhysicalDataTail.TryDequeue(out _)) ;
        }

        //public void UpdatePlotData(double offset, Axis sender, eLOCAL_STATUS status = eLOCAL_STATUS.LIVEUPDATE) //value type 에 따라 달라진 값 푸시
        //{
        //    switch (status)
        //    {
        //        case eLOCAL_STATUS.LIVEUPDATE:
        //            PushUpdatesOnLive(sender.ActualMinimum - offset, sender.ActualMaximum + offset);
        //            break;
        //        case eLOCAL_STATUS.PAUSED:
        //        case eLOCAL_STATUS.STOPPED:
        //            PushUpdatesOnStop(sender.ActualMinimum - offset, sender.ActualMaximum + offset);
        //            break;
        //    }
        //}



        public void UpdatePlotData(ViewPortInfo viewPortInfo, eLOCAL_STATUS status = eLOCAL_STATUS.LIVEUPDATE)
        {
            switch (status)
            {
                case eLOCAL_STATUS.LIVEUPDATE:
                    PushUpdatesOnLive(viewPortInfo);
                    break;
                case eLOCAL_STATUS.PAUSED:
                case eLOCAL_STATUS.STOPPED:
                    PushUpdatesOnStop(viewPortInfo);
                    break;
            }
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

        public void BuildStopChunks(RawChunk[] rawChunks)
        {
            for (int i = 0; i < rawChunks.Length; i++)
            {
                if (rawChunks[i] != null && rawChunks[i].HasData)
                {
                    StopChunks[i] = new StopChunk(SignalDataModel, rawChunks[i].StartTime, rawChunks[i].EndTime, rawChunks[i].Data);
                }
                else
                {
                    StopChunks[i] = new StopChunk(rawChunks[i].StartTime, rawChunks[i].StartTime);
                }
            }
        }

        public void BuildPrevChunk(RawChunk rawChunk)
        {
            StopChunks[2] = StopChunks[1];
            StopChunks[1] = StopChunks[0];
            StopChunks[0] = new StopChunk(SignalDataModel, rawChunk.StartTime, rawChunk.EndTime, rawChunk.Data);
        }

        public void BuildNextChunk(RawChunk rawChunk)
        {
            StopChunks[0] = StopChunks[1];
            StopChunks[1] = StopChunks[2];
            StopChunks[2] = new StopChunk(SignalDataModel, rawChunk.StartTime, rawChunk.EndTime, rawChunk.Data);
        }
        #endregion

        #region SubMethods
        private void PushUpdatesOnLive(ViewPortInfo viewPortInfo)
        {
            double min = viewPortInfo.XMin - viewPortInfo.Offset;
            double max = viewPortInfo.XMax + viewPortInfo.Offset;
            using (BufferLease<GraphDataPoint> snapshot = LiveChunk.SliceByTime(ValueType, min, max))
            {
                GraphRenderModel.UpdateSeries(snapshot, viewPortInfo);
            }
        }

        private void PushUpdatesOnStop(ViewPortInfo viewPortInfo)
        {
            double min = viewPortInfo.XMin;
            double max = viewPortInfo.XMax;
            int fullSize = 0;
            SliceSegment<GraphDataPoint>[] sliceSegments = new SliceSegment<GraphDataPoint>[StopChunks.Length];
            GraphDataPoint? bestleft = null, bestright = null;
            bool isBridged = false;

            for (int i = 0; i < StopChunks.Length; i++)
            {
                if (StopChunks[i] == null || !StopChunks[i].HasData)
                {
                    sliceSegments[i] = SliceSegment<GraphDataPoint>.Empty;
                    continue;
                }

                SliceResult<GraphDataPoint> result = StopChunks[i].GetSegment(ValueType, min, max);

                sliceSegments[i] = result.Slice;
                if (result.HasData)
                {
                    fullSize += result.Slice.Count;
                }

                if (result.HasLeft)
                {
                    if (!bestleft.HasValue || bestleft.Value.X < result.Left.X) bestleft = result.Left;
                }

                if (result.HasRight)
                {
                    if (!bestright.HasValue || bestright.Value.X > result.Right.X) bestright = result.Right;
                }
            }

            if (fullSize <= 0) isBridged = true;
            if (bestleft != null) fullSize++;
            if (bestright != null) fullSize++;

            using (BufferLease<GraphDataPoint> snapshot = BufferLease<GraphDataPoint>.FromPooled(fullSize))
            {
                if (bestleft != null) snapshot.AppendData((GraphDataPoint)bestleft);
                for(int i = 0; i < sliceSegments.Length; i++)
                {
                    if (sliceSegments[i].IsEmpty) continue;
                    snapshot.AppendRange(sliceSegments[i].Array, sliceSegments[i].Offset, sliceSegments[i].Count);
                }
                if (bestright != null) snapshot.AppendData((GraphDataPoint)bestright);
                GraphRenderModel.UpdateSeries(snapshot, viewPortInfo, isBridged);
            }
        }

       

        private double? GetNearest_RawValue(double xValue)
        {
            for (int i = 0; i < StopChunks.Length; i++)
            {
                if (StopChunks[i].StartTime > xValue || StopChunks[i].EndTime < xValue)
                {
                    continue;
                }

                if (StopChunks[i] == null || !StopChunks[i].HasData)
                {
                    break;
                }

                return StopChunks[i].GetNearest_Raw(xValue);
            }
            return null;
        }
        private double? GetNearest_PhysicalValue(double xValue)
        {
            for (int i = 0; i < StopChunks.Length; i++)
            {
                if (StopChunks[i] == null || StopChunks[i].StartTime > xValue || StopChunks[i].EndTime < xValue)
                {
                    continue;
                }

                if (StopChunks[i] == null || !StopChunks[i].HasData)
                {
                    break;
                }

                return StopChunks[i].GetNearest_Phys(xValue);
            }
            return null;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
