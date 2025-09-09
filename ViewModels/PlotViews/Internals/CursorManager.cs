using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.ViewModels.PlotViews.Internals
{
    public class CursorManager
    {
        private readonly PlotModel PlotModel;
        private readonly string XAxisKey;

        public CursorManager(PlotModel plotModel, Axis xAxis)
        {
            PlotModel = plotModel;
            XAxisKey = xAxis.Key;

            //set cursors
            MeasureCursor = CreateDefaultCursor();
            PivotCursor = CreateDefaultCursor();
            PivotCursor.Text = "Pivot";
            TargetCursor = CreateDefaultCursor();
            TargetCursor.Text = "Target";

            MeasureLines.Add(MeasureCursor);
            PivotLines.Add(PivotCursor);
            TargetLines.Add(TargetCursor);
        }

        private LineAnnotation MeasureCursor { get; set; }
        private LineAnnotation TargetCursor { get; set; }
        private LineAnnotation PivotCursor { get; set; }

        private readonly List<LineAnnotation> MeasureLines = new List<LineAnnotation>();
        private readonly List<LineAnnotation> TargetLines = new List<LineAnnotation>();
        private readonly List<LineAnnotation> PivotLines = new List<LineAnnotation>();

        public double PivotXPosition
        {
            get => PivotCursor.X;
        }

        private LineAnnotation CreateDefaultCursor(string yAxisKey = null)
        {
            return new LineAnnotation
            {
                XAxisKey = XAxisKey,
                YAxisKey = yAxisKey,
                Type = LineAnnotationType.Vertical,
                Layer = AnnotationLayer.AboveSeries,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2,
                Color = OxyColors.Black,
                TextOrientation = AnnotationTextOrientation.Horizontal,
                TextVerticalAlignment = VerticalAlignment.Top,
                TextHorizontalAlignment = HorizontalAlignment.Right,
                TextPadding = 3,
                TextMargin = 2
            };
        
        }

        public List<LineAnnotation> GetMeasureLines(ePAGE_TYPE pageType)
        {
            if(pageType == ePAGE_TYPE.SEPARATE_Y)
            {
                return MeasureLines;
            }
            return new List<LineAnnotation> { MeasureCursor };
        }

        public List<LineAnnotation> GetTargetLines(ePAGE_TYPE pageType)
        {
            if (pageType == ePAGE_TYPE.SEPARATE_Y)
            {
                return TargetLines;
            }
            return new List<LineAnnotation> { TargetCursor };
        }

        public List<LineAnnotation> GetPivotLines(ePAGE_TYPE pageType)
        {
            if (pageType == ePAGE_TYPE.SEPARATE_Y)
            {
                return PivotLines;
            }
            return new List<LineAnnotation> { PivotCursor };
        }

        public void SetMeasureX(double x)
        {
            foreach(var annotation in MeasureLines)
            {
                annotation.X = x;
            }
        }

        public void SetTargetX(double x)
        {
            foreach(var annotation in TargetLines)
            {
                annotation.X = x;
            }
        }

        public void SetPivotX(double x)
        {
            foreach(var annotation in PivotLines)
            {
                annotation.X = x;
            }
        }

        public void OnGraphCollectionChanged(List<GraphModel> graphs)
        {
            MeasureLines.Clear();
            MeasureLines.Add(MeasureCursor);

            TargetLines.Clear();
            TargetLines.Add(TargetCursor);

            PivotLines.Clear();
            PivotLines.Add(PivotCursor);
            
            foreach(var graph in graphs)
            {
                string key = graph.GraphRenderModel.YAxis.Key;
                MeasureLines.Add(CreateDefaultCursor(key));
                TargetLines.Add(CreateDefaultCursor(key));
                PivotLines.Add(CreateDefaultCursor(key));
            }
        }
    }
}
