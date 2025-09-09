using HPT.Common.Models;
using HPT.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OxyTest.Events
{
    public class EventDispatcher
    {
        public EventDispatcher() { }

        public void Request_PastData(object sender, PastDataRequestArgs args)
        {
            EventHandlerManager.Default.Send("PastDataReq", sender, args);
        }

        public void Request_DataRange(object sender, DataRangeRequestArgs args)
        {
            EventHandlerManager.Default.Send("DataRangeReq", sender, args);
        }

        public void Request_TimeRangeData(object sender, TimeRangeEventArgs args)
        {
            EventHandlerManager.Default.Send("TimeRangeReq", sender, args);
        }
    }
}
