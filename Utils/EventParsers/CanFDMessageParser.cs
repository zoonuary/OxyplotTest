using HPT.Common.Models;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Utils.EventParsers
{
    class CanFDMessageParser: IEventDataParser
    {
        public EventModel Parse(EventData data)
        {
            if (data is CanFdDataFrame dataFrame)
            {
                return new EventModel(eEVENT_BEHAVIOR_TYPE.ONEVENT, dataFrame.Type, dataFrame.Index, dataFrame.MsgId, dataFrame.DLC, dataFrame.Data, dataFrame.TimeStamp, dataFrame.IsExtended);
            }
            throw new NotSupportedException($"[CanMessageParser] Type mismatched");
        }
    }
}
