using HPT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Event
{
    public class DataSlotModel
    {
        public DataSlotModel(List<EventData> dataList)
        {
            int length = dataList.Count;
            if(length > 0)
            {
                var first = dataList[0];
                FirstTime = Math.Floor((double)(first.TimeStamp * 1e-9) * 1e6) / 1e6;
                FirstDataIndex = first.Index;

                var last = dataList[length - 1];
                LastTime = Math.Floor((double)(last.TimeStamp * 1e-9) * 1e6) / 1e6;
            }
        }

        public ulong FirstDataIndex;
        public double FirstTime;
        public double LastTime;
        public List<EventData> EventDataList;
    }
}
