using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Event
{
    public class GraphSyncEventModel
    {
        public GraphSyncEventModel(object instanceID, double min, double max)
        {
            InstanceID = instanceID;
            Xaxis_ActualMinimum = min;
            Xaxis_ActualMaximum = max;
        }

        public object InstanceID { get; set; }
        public double Xaxis_ActualMinimum { get; set; }
        public double Xaxis_ActualMaximum { get; set; }
    }
}
