using HPT.Common.Models;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    public class RawChunk
    {

        public RawChunk()
        {
            StartTime = 0;
            EndTime = 0;
            Data = null;
            HasData = false;
        }

        public RawChunk(double startTime, double endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
            Data = null;
            HasData = false;
        }

        public RawChunk(double startTime, double endTime, EventModel[] data)
        {
            StartTime = startTime;
            EndTime = endTime;
            Data = data;
            if (data == null || data.Count() <= 0)
                HasData = false;
            else
                HasData = true;
        }

        public double StartTime { get; }

        public double EndTime { get; }

        public EventModel[] Data { get; }

        public bool HasData { get; }

        public double TimeAtRatio(double ratio) => StartTime + ratio * (EndTime - StartTime);
    }
}
