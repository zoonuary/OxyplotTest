using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OxyTest.ViewModels.PlotViews.Internals
{
    public class CursorData
    {
		public string Name { get; set; }
		public string Value { get; set; }
		public Color Color { get; set; }
		public Brush BrushColor => new SolidColorBrush(Color);
		public CursorData(string name, string value)
		{
			Name = name;
			Value = value;
			Color = Colors.Black;
		}

		public CursorData(string name, string value , Color color)
        {
			Name = name;
			Value = value;
			Color = color;
        }
	}
}
