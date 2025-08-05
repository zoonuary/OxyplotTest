using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OxyTest.Models.Graph
{
    public class GraphRenderModel : INotifyPropertyChanged
    {
        /*
		 * GraphRenderModel에선 화면에 그려질 때만 필요한 데이터들을 취급
		 */
        public GraphRenderModel(object tag, SignalDataModel signal)
        {
            SignalDataModel = signal;
            Tag = tag;

            lineSeries = new LineSeries() { Tag = tag , YAxisKey = tag.ToString() , StrokeThickness = 2};
            //areaSeries = new AreaSeries() { Tag = tag , YAxisKey = tag.ToString() };
            //barSeries = new BarSeries() { Tag = tag, YAxisKey = tag.ToString() };
            scatterSeries = new ScatterSeries() { Tag = tag, YAxisKey = tag.ToString() };

            
            SetYAxis();
        }

        #region Properties
        private SignalDataModel SignalDataModel { get; }
        private object Tag { get; }

        private readonly LineSeries lineSeries;
        private readonly AreaSeries areaSeries;
        private readonly BarSeries barSeries;
        private readonly ScatterSeries scatterSeries;

        private ePLOT_MODE plotMode;
        public ePLOT_MODE PlotMode
        {
            get => plotMode;
            set
            {
                if (plotMode != value)
                {
                    plotMode = value;
                    OnLineTypeCHanged(value);
                    NotifyPropertyChanged(nameof(PlotMode));
                }
            }
        }

        private bool visible;
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;

                    YAxis_Raw.IsAxisVisible = value;
                    YAxis_Physical.IsAxisVisible = value;
                    lineSeries.IsVisible = value;
                    //areaSeries.IsVisible = value;
                    //barSeries.IsVisible = value;
                    //scatterSeries.IsVisible = value;
                    NotifyPropertyChanged(nameof(Visible));
                }
            }
        }

        //basecolor가 바뀌면 plot color, axis color 전부 바뀌어야함.
        private Color baseColor;
        public Color BaseColor
        {
            get => baseColor;
            set
            {
                if (baseColor != value)
                {
                    baseColor = value;
                    ChangeSeriesColor(value);
                    ChaangeAxisColor(YAxis_Raw, value);
                    ChaangeAxisColor(YAxis_Physical, value);
                    NotifyPropertyChanged(nameof(BaseColor));
                }
            }
        }

        private eVALUE_TYPE valueType;
        public eVALUE_TYPE ValueType
        {
            get => valueType;
            set
            {
                if (valueType != value)
                {
                    valueType = value;
                    SetYAxis(value);
                    NotifyPropertyChanged(nameof(ValueType));
                }
            }
        }

        private LinearAxis yAxis;
        public LinearAxis YAxis
        {
            get => yAxis;
            set
            {
                if (yAxis != value)
                {
                    yAxis = value;
                    NotifyPropertyChanged(nameof(YAxis));
                }
            }
        }

        private bool isTitleVisible;
        public bool IsTitleVisible
        {
            get => isTitleVisible;
            set
            {
                if (isTitleVisible != value)
                {
                    isTitleVisible = value;
                    if (isTitleVisible)
                    {
                        YAxis.Title = Label;
                    }
                    else
                    {
                        YAxis.Title = string.Empty;
                    }
                    NotifyPropertyChanged(nameof(YAxis));
                    NotifyPropertyChanged(nameof(IsTitleVisible));
                }
            }
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    OnLineTypeCHanged(PlotMode);
                    NotifyPropertyChanged(nameof(Selected));
                }
            }
        }

        public Series CurrentSeries
        {
            get
            {
                switch (PlotMode)
                {
                    case ePLOT_MODE.LINE:
                        return lineSeries;
                    case ePLOT_MODE.BAR:
                        //return barSeries;
                        break;
                    case ePLOT_MODE.AREA:
                        //return areaSeries;
                        break;
                    case ePLOT_MODE.POINT:
                        return lineSeries;
                    case ePLOT_MODE.LINE_POINT:
                        return lineSeries;
                }
                return null;
            }
        }

        public double physicalMin { get; private set; }
        public double physicalMax { get; private set; }
        public double RawMin { get; private set; }
        public double RawMax { get; private set; }

        private LinearAxis YAxis_Physical { get; set; }
        private double YAxis_PhysicalMin { get; set; }
        private double YAxis_PhysicalMax { get; set; }

        private LinearAxis YAxis_Raw { get; set; }
        private double YAxis_RawMin { get; set; }
        private double YAxis_RawMax { get; set; }

        private string Label { get; }

        #endregion

        #region Methods
        /// <summary>
        /// do lazy initialize
        /// </summary>
        public void Initialize()
        {
            PlotMode = ePLOT_MODE.LINE;
            Visible = true;
            BaseColor = Colors.Red;
            IsTitleVisible = isTitleVisible;
        }

        public void UpdateSeries(List<GraphDataPoint> data)
        {
            //RenderModel에서 업데이트되는 dataseries는 보여지는 부분에 대해서만이지, 전체 series에 대해서는 아님
            switch (PlotMode)
            {
                case ePLOT_MODE.LINE:
                    UpdateLineSeries(data);
                    break;
                case ePLOT_MODE.BAR:
                    //UpdateBarSeires(data);
                    break;
                case ePLOT_MODE.AREA:
                    //UpdateAreaSeries(data);
                    break;
                case ePLOT_MODE.POINT:
                    UpdateLineSeries(data);
                    break;
                case ePLOT_MODE.LINE_POINT:
                    UpdateLineSeries(data);
                    break;
            }
        }

        public void FitYAxis()
        {
            YAxis_Physical.Zoom(YAxis_PhysicalMin, YAxis_PhysicalMax);
            YAxis_Raw.Zoom(YAxis_RawMin, YAxis_RawMax);
            NotifyPropertyChanged(nameof(YAxis));
        }

        public void SetGridLineVisible(bool visibility)
        {
            if (visibility)
            {
                YAxis_Physical.MajorGridlineStyle = LineStyle.Solid;
                YAxis_Physical.MinorGridlineStyle = LineStyle.Dot;
                YAxis_Raw.MajorGridlineStyle = LineStyle.Solid;
                YAxis_Raw.MinorGridlineStyle = LineStyle.Dot;
            }
            else
            {
                YAxis_Physical.MajorGridlineStyle = LineStyle.None;
                YAxis_Physical.MinorGridlineStyle = LineStyle.None;
                YAxis_Raw.MajorGridlineStyle = LineStyle.None;
                YAxis_Raw.MinorGridlineStyle = LineStyle.None;
            }
        }


        #endregion

        #region SubMethods
        private void SetYAxis()
        {
            var PhysicalRange = Initialize_PhysicalRange(SignalDataModel.Length, SignalDataModel.Factor, SignalDataModel.Offset, SignalDataModel.IsUnsigned);
            physicalMin = PhysicalRange.Item1;
            physicalMax = PhysicalRange.Item2;
            var RawRange = Initialize_RawRange(SignalDataModel.Length, SignalDataModel.IsUnsigned);
            RawMin = RawRange.Item1;
            RawMax = RawRange.Item2;

            YAxis_Physical = Initialize_PhysicalYAxis(Tag, SignalDataModel);
            YAxis_Raw = Initialize_RawAxis(Tag, SignalDataModel);
            YAxis = YAxis_Physical;
        }

        private Tuple<double, double> Initialize_PhysicalRange(int length, double factor, double offset, bool Unsigned)
        {
            double max = ((1 << length) - 1) * factor + offset; //2bit length
            double min;
            if (Unsigned) //최솟값 = offset 고정
            {
                min = offset;
                var PhysicalJumpAmount = Math.Abs((max - min) * 0.1);
                return new Tuple<double, double>(min - PhysicalJumpAmount, max + PhysicalJumpAmount);
            }
            else
            {
                min = -((1 << length) - 1) * factor + offset;
                var PhysicalJumpAmount = Math.Abs((max - min) * 0.1);
                return new Tuple<double, double>(min - PhysicalJumpAmount, max + PhysicalJumpAmount);
            }
        }

        private Tuple<double, double> Initialize_RawRange(int length, bool Unsigned)
        {
            double max = (1 << length) - 1;
            double min = 0;
            if (Unsigned)
            {
                var RawJumpAmount = max * 0.1;
                return new Tuple<double, double>(min - RawJumpAmount, max + RawJumpAmount);
            }
            else
            {
                min = -((1 << length) - 1);
                var RawJumpAmount = Math.Abs((max - min) * 0.1);
                return new Tuple<double, double>(min - RawJumpAmount, max + RawJumpAmount);
            }
        }

        private LinearAxis Initialize_PhysicalYAxis(object tag, SignalDataModel signal)
        {
            var valueDescriptions = signal.ValueDescriptions;
            var axis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Tag = tag,
                Key = tag.ToString(),
                Title = signal.Name,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                LabelFormatter = input =>
                {
                    if (input % 1 == 0 && valueDescriptions.ContainsKey((int)input))
                    {
                        return valueDescriptions[(int)input];
                    }
                    return input.ToString();
                }
            };
            axis.Zoom(physicalMin, physicalMax);
            YAxis_PhysicalMin = physicalMin;
            YAxis_PhysicalMax = physicalMax;
            return axis;
        }

        private LinearAxis Initialize_RawAxis(object tag, SignalDataModel signal)
        {
            var axis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Tag = tag,
                Key = tag.ToString(),
                Title = signal.Name,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = RawMin,
                Maximum = RawMax
            };
            YAxis_RawMin = RawMin;
            YAxis_RawMax = RawMax;
            axis.Zoom(RawMin, RawMax);
            return axis;
        }

        private void ChangeSeriesColor(Color color)
        {
            OxyColor oxyColor = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
            lineSeries.Color = oxyColor;
            lineSeries.MarkerFill = oxyColor;
            lineSeries.MarkerStroke = oxyColor;
            //scatterSeries.MarkerFill = oxyColor;
            //areaSeries.Color = oxyColor;
            //areaSeries.Fill = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
            //barSeries.FillColor = oxyColor;
        }

        private void ChaangeAxisColor(LinearAxis axis, Color color)
        {
            byte MinorGridAlpha = 32;
            byte MajorGridAlpha = 64;
            OxyColor oxyColor = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
            axis.AxislineColor = oxyColor;
            axis.TicklineColor = oxyColor;
            axis.TextColor = oxyColor;
            axis.TitleColor = oxyColor;
            axis.MinorGridlineColor = OxyColor.FromArgb(MinorGridAlpha, color.R, color.G, color.B);
            axis.MajorGridlineColor = OxyColor.FromArgb(MajorGridAlpha, color.R, color.G, color.B);
        }

        private void OnLineTypeCHanged(ePLOT_MODE plotmode)
        {
            switch (plotmode)
            {
                case ePLOT_MODE.LINE:
                    SetLineSeriesToLine();
                    break;
                case ePLOT_MODE.AREA:
                    break;
                case ePLOT_MODE.BAR:
                    break;
                case ePLOT_MODE.POINT:
                    SetLineSeriesToPoint();
                    break;
                case ePLOT_MODE.LINE_POINT:
                    SetLineSeriesToLinePoint();
                    break;
            }
        }

        private void SetYAxis(eVALUE_TYPE valueType)
        {
            FitYAxis();
            switch (valueType)
            {
                case eVALUE_TYPE.PHYSICAL:
                    YAxis = YAxis_Physical;
                    break;
                case eVALUE_TYPE.RAW:
                    YAxis = YAxis_Raw;
                    break;
            }
        }

        private void UpdateLineSeries(List<GraphDataPoint> data)
        {
            int dataCount = data.Count();
            var points = new List<DataPoint>(dataCount);
            for (int i = 0; i < dataCount; i++)
            {
                points.Add(new DataPoint(data[i].X, data[i].Y));
            }
            lineSeries.Points.Clear();
            lineSeries.Points.AddRange(points); //addrange로 통으로 넣는게 더 라이브러리에 최적화되어있음
        }

        private void SetLineSeriesToLine() //lineseries를 pointseries처럼 보이게
        {
            if (Selected)
            {
                lineSeries.StrokeThickness = 3;
            }
            else
            {
                lineSeries.StrokeThickness = 2;
            }
            lineSeries.MarkerType = MarkerType.None;
        }

        private void SetLineSeriesToPoint() //lineseires를 lineseries처럼 보이게
        {
            if (Selected)
            {
                lineSeries.MarkerType = MarkerType.Circle;
                lineSeries.MarkerSize = 4;
            }
            else
            {
                lineSeries.MarkerType = MarkerType.Circle;
                lineSeries.MarkerSize = 3;
            }
            lineSeries.StrokeThickness = 0;
        }

        private void SetLineSeriesToLinePoint()
        {
            if (Selected)
            {
                lineSeries.StrokeThickness = 3;
                lineSeries.MarkerType = MarkerType.Circle;
                lineSeries.MarkerSize = 4;
            }
            else
            {
                lineSeries.StrokeThickness = 2;
                lineSeries.MarkerType = MarkerType.Circle;
                lineSeries.MarkerSize = 3;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
