using HPT.Common.Models;
using OxyTest.Models.Event;
using OxyTest.Utils.EventParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Utils
{


    public class EventDataParser
    {
        private static EventDataParser instance;
        public static EventDataParser Instance
        {
            get
            {
                if (instance == null)
                    instance = new EventDataParser();
                return instance;
            }
        }

        private readonly Dictionary<EventType, IEventDataParser> EventHandlerMap;

        public EventDataParser()
        {
            //파싱 추가시 추가할것 (ex: CANFD Message Frame)
            EventHandlerMap = new Dictionary<EventType, IEventDataParser>
            {
                {EventType.CAN_MESSAGE, new CanMessageParser()},
                {EventType.CAN_ERR_MESSAGE, new CanErrMessageParser()},
                {EventType.CAN_FD_MESSAGE, new CanFDMessageParser()},
                {EventType.CAN_FD_ERR_MESSAGE, new CanFDErrMessageParser()},
            };
        }

        public EventModel Parse(EventData data)
        {
            if (EventHandlerMap.TryGetValue(data.Type, out var handler))
            {
                return handler.Parse(data);
            }
            throw new NotSupportedException($"[EventDataParser] Unknown Type {data.Type}");
        }
    }
}
