using DevExpress.Drawing.Compatibility.Internal;
using OxyTest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph.DataModel
{
    public interface IChunk<T>
    {
        SignalDataModel SignalDataModel { get; }
        double StartTime { get; }
        double EndTime { get; }
        BufferLease<T> SliceByTime(eVALUE_TYPE valueType, double start, double end);
    }
}
