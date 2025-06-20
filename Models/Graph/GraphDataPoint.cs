using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
	public class GraphDataPoint
	{
		public DateTime X { get; set; }
		public double Y { get; set; }
		public double? Size { get; set; } //scatter
		public int? CategoryIndex { get; set; } //bar
		public string groupKey { get; set; }
	}
}
