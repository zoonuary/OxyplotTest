using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.ViewModels.GridViews.Internals
{
    public class GridPlotType
    {
		public string DisplayName { get; set; }
		public ePLOT_MODE PlotMode { get; set; }
		public GridPlotType(string displayname, ePLOT_MODE plotmode)
		{
			DisplayName = displayname;
			PlotMode = plotmode;
		}
	}
}
