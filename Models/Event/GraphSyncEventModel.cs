using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Event
{

    //추후 확장한다면, 외부에서 plot을 컨트롤할 수 있는방향으로 확장 고려 ex) PlotControlEventModel
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
