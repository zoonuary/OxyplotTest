using HPT.Common.Models;
using HPT.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Events
{
    public class EventDispatcher
    {
        public EventDispatcher() { }

        public void Request_PastData(object sender, PastDataRequestArgs pastData)
        {
            EventHandlerManager.Default.Send("PastDataReq", this, pastData);
        }

        public void Request_DataRange(object sender, DataRangeRequestArgs rangeRequestArgs)
        {
            EventHandlerManager.Default.Send("DataRangeReq", this, rangeRequestArgs);
        }
    }
}
