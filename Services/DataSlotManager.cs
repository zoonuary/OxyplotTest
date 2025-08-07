using HPT.Common.Models;
using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Services
{
    public class DataSlotManager
    {
        public DataSlotManager(EventDispatcher eventDispatcher, GraphData graphData)
        {
            EventDispatcher = eventDispatcher;
            GraphData = graphData;
        }

        private EventDispatcher EventDispatcher { get; }

        private GraphData GraphData { get; }

        private ulong MaxSlotCount = 4;

        private ulong SlotDataSize = 5000;

        //private ulong currentIdx = 0;

        List<DataSlotModel> SlotList = new();// 리스트는 언제든 새로 할담됨. readonly불가, 바인딩 불가

        public void CreateNewSlot(double lastEventTime)
        {
            ulong currentIdx = 0;
            //if(SlotList.Count < 1)
            //{
            //    currentIdx = 0;
            //}
            //else
            //{
            //    currentIdx = SlotList[SlotList.Count - 1].FirstDataIndex + SlotDataSize;
            //}

            PastDataRequestArgs args = new PastDataRequestArgs($"Graph_{GraphData.InstanceID}", currentIdx, (int)SlotDataSize);
            EventDispatcher.Request_PastData(this, args);

            SlotList.Add(new DataSlotModel(args.EventDataList));
        }

        public void OnStop()
        {
            DataRangeRequestArgs args = new DataRangeRequestArgs();
            EventDispatcher.Request_DataRange(this, args);

            ulong requestSize = SlotDataSize * (MaxSlotCount - 1) + args.EndIndex % SlotDataSize;
            ulong startIdx = Math.Max(0, args.EndIndex - requestSize);

            SlotList.Clear();
            for(ulong i = 1; i <= MaxSlotCount; i++)
            {
                PastDataRequestArgs dataArgs = RequestDataArgs(startIdx * i, (int)SlotDataSize);
                SlotList.Add(new DataSlotModel(dataArgs.EventDataList));
            }
        }


        private ulong GetStartIdx(ulong endIndex, ulong slotDataSize, ulong maxSlotCount)
        {
            if (endIndex <= slotDataSize * maxSlotCount)
                return 0;
            ulong remainder = endIndex % slotDataSize;
            ulong requestSize = (slotDataSize * maxSlotCount) + remainder;
            return endIndex - requestSize;
        }


        private void SlotList_AddFirst(DataSlotModel model)
        {
            var newList = new List<DataSlotModel>(1 + SlotList.Count);
            newList.Add(model);
            newList.AddRange(SlotList);
            SlotList = newList;
        }

        private void SlotList_AddLast(DataSlotModel model)
        {
            SlotList.Add(model);
        }

        private PastDataRequestArgs RequestDataArgs(ulong requestPoint, int size)
        {
            var args = new PastDataRequestArgs($"Graph_{GraphData.InstanceID}", requestPoint, size);
            EventDispatcher.Request_PastData(this, args);
            return args;
        }
    }
}
