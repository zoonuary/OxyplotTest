using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
    public class ViewPortInfo
    {
        public double Offset { get; }
        public double XMin { get; }
        public double XMax { get; }

        public double OffsetMin => XMin - Offset;
        public double OffsetMax => XMax + Offset;

        public int PixelWidth { get; }
        public OxyRect PlotArea { get; }
        public ViewPortInfo(double xmin, double xmax, int pixelwidth, OxyRect plotarea, double offset)
        {
            XMin = xmin;
            XMax = xmax;
            PixelWidth = pixelwidth;
            PlotArea = plotarea;
            Offset = offset;
        }

        public ViewPortInfo(double xmin, double xmax, int pixelwidth, OxyRect plotarea)
        {
            XMin = xmin;
            XMax = xmax;
            PixelWidth = pixelwidth;
            PlotArea = plotarea;
            double offsetMinBound = 0.1;
            Offset = Math.Max((xmax - xmin) * 0.2, 0.1);
        }
    }
}
