using HPT.Common.Models;
using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using OxyTest.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxyTest.Services
{
    public class DataChunkManager
    {
        //얘가 현재 x축 상황 및 상태(liveupdate , stop) 
        //데이터가 더 필요한지를 판단할 수 있어야함 
        //Graphmodel에게 맡겨버리면 model이 많을수록 불리하고 또 x축은 고정되어 이동함으로 한번에 관리되어야할필요가 있음

        public DataChunkManager(GraphData graphData, EventDispatcher eventDispatcher, GraphEventHandler graphEventHandler)
        {
            GraphData = graphData;
            EventDispatcher = eventDispatcher;
            GraphEventHandler = graphEventHandler;

            lock (_lock)
            {
                RawChunks[0] = new RawChunk();
                RawChunks[1] = new RawChunk();
                RawChunks[2] = new RawChunk();
            }

            Loop = Task.Run(WorkAsynk);

            GraphData.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GraphData.eLOCAL_STATUS))
                {
                    switch (GraphData.eLOCAL_STATUS)
                    {
                        case eLOCAL_STATUS.LIVEUPDATE:
                            lock (_lock)
                            {
                                RawChunks[0] = new RawChunk();
                                RawChunks[1] = new RawChunk();
                                RawChunks[2] = new RawChunk();
                            }
                            break;
                        case eLOCAL_STATUS.PAUSED:
                        case eLOCAL_STATUS.STOPPED:
                            PausedTime = GraphData.LastEventTime;
                            break;
                    }
                }
            };
        }

        private GraphData GraphData { get; }

        private EventDispatcher EventDispatcher { get; }

        private GraphEventHandler GraphEventHandler { get; }

        private RawChunk[] RawChunks { get; } = new RawChunk[3]; //비동기 처리 시 lock 걸어야함

        private double PausedTime { get; set; }

        private object _lock = new object();


        private enum eLOAD_DIR { IDLE = 0, NEXT = 1, PREV = 2 }
        private int isLoading = 0; // 0 : false, 1 : true


        //신호 감시. TCSource.TrySetResult(true)

        private readonly SemaphoreSlim LoopGate = new SemaphoreSlim(0, 1);
        private int isPending;
        private readonly CancellationTokenSource CTS = new();
        private readonly Task Loop;
        private ViewPortInfo SnapshotViewportInfo;
        public Action<ViewPortInfo> ChunkUpdated;


        public async Task<bool> EnsureChunkLoadedAsync(double startTime, double endTime)
        {
            /*
             * Todo : 점프(가지고있는 chunk보다 scope가 과하게 멀어진경우) 고려 필요.
             */


            double axisLen = endTime - startTime;
            eLOAD_DIR dir =
                ShouldLoadNext(axisLen, startTime, RawChunks[1].EndTime) ? eLOAD_DIR.NEXT
                : ShouldLoadPrev(axisLen, startTime, RawChunks[1].StartTime) ? eLOAD_DIR.PREV
                : eLOAD_DIR.IDLE;

            if (dir == eLOAD_DIR.IDLE)
            {
                return false;
            }

            if (System.Threading.Interlocked.CompareExchange(ref isLoading, 1, (int)eLOAD_DIR.IDLE) != (int)eLOAD_DIR.IDLE)
            {
                return false; //isLoading 이 0인 경우 1로 교체되며 원래값 0반환. isLoading이 0이 아니면 아무일도 안하고 원래 값 반환
            }

            try
            {
                RawChunk prev, curr, next;
                eLOAD_DIR final;
                //비행 시작 직전 재평가필요 => 비동기 처리 결과가 Interlocked.CompareExchange 직후 바로 RawChunks에 반영되지 않는다는 보장이 없음
                lock (_lock)
                {
                    prev = RawChunks[0];
                    curr = RawChunks[1];
                    next = RawChunks[2];

                    final =
                        ShouldLoadNext(axisLen, startTime, curr.EndTime) ? eLOAD_DIR.NEXT
                        : ShouldLoadPrev(axisLen, startTime, curr.StartTime) ? eLOAD_DIR.PREV
                        : eLOAD_DIR.IDLE;

                    if (final == eLOAD_DIR.IDLE)
                    {
                        return false;
                    }
                }

                if (final == eLOAD_DIR.NEXT)
                {
                    var nextStart = next.EndTime;
                    var nextEnd = nextStart + GraphData.DataChunkSize;
                    var preLoadedChunk = await Task.Run(() =>
                    {
                        return new RawChunk(nextStart, nextEnd, GetChunkArray(nextStart, nextEnd));
                    });
                    lock (_lock) //swap & render
                    {
                        RawChunks[0] = curr;
                        RawChunks[1] = next;
                        RawChunks[2] = preLoadedChunk;
                        foreach (var graph in GraphData.Graphs)
                        {
                            graph.BuildNextChunk(preLoadedChunk);
                        }
                    }
                    return true;
                }
                else // final == eLOAD_DIR.PREV
                {
                    var prevEnd = prev.StartTime;
                    var prevStart = prevEnd - GraphData.DataChunkSize;
                    var preLoadedChunk = await Task.Run(() =>
                    {
                        return new RawChunk(prevStart, prevEnd, GetChunkArray(prevStart, prevEnd));
                    });
                    lock (_lock)
                    {
                        RawChunks[0] = preLoadedChunk;
                        RawChunks[1] = prev;
                        RawChunks[2] = curr;
                        foreach (var graph in GraphData.Graphs)
                        {
                            graph.BuildPrevChunk(preLoadedChunk);
                        }
                    }

                    return true;
                }
            }
            finally
            {
                Volatile.Write(ref isLoading, (int)eLOAD_DIR.IDLE);
            }
        }


        public void EnsureCoverage(ViewPortInfo viewPortInfo)
        {
            Volatile.Write(ref SnapshotViewportInfo, viewPortInfo);
            if (Interlocked.Exchange(ref isPending, 1) == 0)
            {
                LoopGate.Release();
            }
        }

        private async Task WorkAsynk()
        {
            while (!CTS.Token.IsCancellationRequested)
            {
                await LoopGate.WaitAsync(CTS.Token).ConfigureAwait(false);
                Interlocked.Exchange(ref isPending, 0); 
                try
                {
                    while (true)
                    {
                        var viewport = Volatile.Read(ref SnapshotViewportInfo);
                        bool result = await EnsureChunkLoadedAsync(viewport.XMin, viewport.XMax).ConfigureAwait(false);
                        if (result)
                        {
                            ChunkUpdated?.Invoke(viewport);
                        }
                        //작업 중 새 요청이 온 경우, 한 번 더
                        if (Interlocked.Exchange(ref isPending, 0) == 1)
                        {
                            LoopGate.Wait(0);
                            continue;
                        }
                        break;
                    }
                }
                finally { }
            }
        }


        private bool ShouldLoadNext(double axisLen, double axisMin, double ChunkEnd)
        {
            double rEnd = (ChunkEnd - axisMin) / axisLen;
            return rEnd <= 0.25;
        }

        private bool ShouldLoadPrev(double axisLen, double axisMin, double ChunkStart)
        {
            double rStart = (ChunkStart - axisMin) / axisLen;
            return rStart >= 0.75;
        }

        public void OnLocalStatus_paused(double chunkSize, double lastEventTime, ViewPortInfo viewPortInfo)
        {
            if (lastEventTime < chunkSize) //just load 0~chunkSize
            {
                RawChunks[0] = new RawChunk(chunkSize * -1, 0);
                RawChunks[1] = new RawChunk(0, chunkSize, GetChunkArray(0, lastEventTime));
                RawChunks[2] = new RawChunk(chunkSize, chunkSize + chunkSize);
            }
            else //check start
            {
                var currStart = Math.Floor(viewPortInfo.XMax / chunkSize) * chunkSize;
                var currEnd = currStart + chunkSize;
                RawChunks[1] = new RawChunk(currStart, currEnd, GetChunkArray(currStart, viewPortInfo.XMax));

                var leftStart = currStart - chunkSize;
                var leftEnd = currStart;
                RawChunks[0] = new RawChunk(leftStart, leftEnd, GetChunkArray(leftStart, leftEnd));

                var rightStart = currEnd;
                var rightEnd = currEnd + chunkSize;
                RawChunks[2] = new RawChunk(rightStart, rightEnd);
            }

            foreach (var graph in GraphData.Graphs)
            {
                graph.BuildStopChunks(RawChunks);
                graph.UpdatePlotData(viewPortInfo, eLOCAL_STATUS.PAUSED);
            }
        }



        //private List<EventData> GetChunk(double startTime, double endTime)
        //{
        //    TimeRangeEventArgs args = new TimeRangeEventArgs(startTime, endTime);
        //    EventDispatcher.Request_TimeRangeData(this, args);
        //    return args.eventDatas;
        //}

        //private EventModel[] GetChunkArray(double startTime, double endTime)
        //{
        //    TimeRangeEventArgs args = new TimeRangeEventArgs(0.0, endTime);
        //    EventDispatcher.Request_TimeRangeData(this, args);

        //    int count = args.eventDatas.Count;
        //    EventModel[] events = new EventModel[count];
        //    for (int i = 0; i < count; i++)
        //    {
        //        events[i] = EventDataParser.Instance.Parse(args.eventDatas[i]);
        //    }
        //    return events;
        //}

        /// <summary>
        /// 시작시간 ~ 끝 시간을 받아와 그에 해당하는 데이터를 배열로 반환
        /// </summary>
        /// <param name="startTime"> 시작 시간 </param>
        /// <param name="endTime"> 끝 시간 </param>
        /// <param name="reqSize"> 시작 시간으로부터 요구할 데이터 갯수 </param>
        /// <param name="remaining"> 최대 반복횟수(무한루프 방지) </param>
        /// <returns></returns>
        private EventModel[] GetChunkArray(double startTime, double endTime, int reqSize = 10000, int remaining = 100)
        {
            if (startTime > PausedTime) //reqtime < 마지막 이벤트시간
            {
                return new EventModel[0];
            }

            if (endTime > PausedTime)
            {
                endTime = PausedTime;
            }

            try
            {
                double next = startTime;
                var list = new List<EventData>();
                while (true)
                {
                    if (remaining-- <= 0)
                    {
                        break; //무한루프 방지(100회제한)
                    }

                    var reqList = ReqDataByTimeSize(next, reqSize);
                    int count = reqList.Count;
                    if (count == 0)
                    {
                        break; //가져온게 없음
                    }

                    list.AddRange(reqList);
                    double lastTimeStamp = ConvertTimestampToDouble(reqList[count - 1]?.TimeStamp);
                    //double lastTimeStamp = Math.Floor((double)(reqList[count - 1]?.TimeStamp * 1e-9) * 1e6) / 1e6;
                    if (count < reqSize || lastTimeStamp >= endTime)
                    {
                        break; //마지막 데이터를 포함해서 가져왔음 || 더이상 가져올 필요가 없음
                    }

                    next = lastTimeStamp;
                }

                int upperbound = UpperBound(list, endTime);
                if (list.Count > 0 && upperbound > 0)
                {
                    EventModel[] result = new EventModel[upperbound];
                    for (int i = 0; i < upperbound; i++)
                    {
                        if (i != 0 && list[i].Index == list[i - 1].Index)
                        {
                            continue; //여러번 호출 시 리스트 중복 제거
                        }

                        result[i] = EventDataParser.Instance.Parse(list[i]);
                    }
                    return result;
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException("disable to read temp file for somehow reason");
            }
            return new EventModel[0];
        }

        private int UpperBound(List<EventData> list, double timeStamp)
        {
            int low = 0;
            int high = list.Count;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (ConvertTimestampToDouble(list[mid].TimeStamp) < timeStamp)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }
            return low;
        }

        private double ConvertTimestampToDouble(ulong? timestamp) => Math.Floor((double)(timestamp * 1e-9) * 1e6) / 1e6;

        private List<EventData> ReqDataByTimeSize(double startTime, int reqSize)
        {
            var args = new PastDataRequestArgs("Graph", startTime, reqSize, DataRequestType.ByTime);
            EventDispatcher.Request_PastData(this, args);
            return args.EventDataList;
        }
    }
} 
