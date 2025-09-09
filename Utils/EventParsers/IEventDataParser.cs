using HPT.Common.Models;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Utils.EventParsers
{
    public interface IEventDataParser
    {
        EventModel Parse(EventData data);
    }
}
