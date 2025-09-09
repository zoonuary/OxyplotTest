using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
	/// <summary>
	/// Series type 확장을 위해(bar, area 등등) 만들어진 전용 타입 클래스.
	/// 필요없다면 OxyPlot.DataPoint 클래스를 그냥 사용하는게 더 바람직함.
	/// </summary>
	public readonly struct GraphDataPoint
	{
		public readonly ulong Index;
		public readonly double X;
		public readonly double Y;
		public GraphDataPoint(ulong index, double x, double y)
        {
			Index = index;
			X = x;
			Y = y;
        }


		//public int Index { get; set; }
		//public double X { get; set; }
		//public double Y { get; set; }
		//public double? Size { get; set; } //scatter, 당장은 사용하지 않음. 
		//public int? CategoryIndex { get; set; } //bar
		//public string groupKey { get; set; }
	}
}
