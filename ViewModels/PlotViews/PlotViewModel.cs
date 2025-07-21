using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyTest.Composition;
using OxyTest.Models.Graph;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace OxyTest.ViewModels
{
    public class PlotViewModel : ViewModelBase
    {
        private GraphCore GraphCore { get; }
        public PlotModel PlotModel { get; }

        private GraphModel CurrentItem = new GraphModel();
        public PlotViewModel(GraphCore graphCore)
        {
            GraphCore = graphCore;
            PlotModel = new PlotModel();
            SetDefaultAxis(PlotModel);

            //update 등록
            GraphCore.SubscribeModelUpdates(UpdatePlotModel);


            var GraphData = graphCore.GraphData;
            GraphData.Graphs_CollectionChanged += OnGraphCollectionChanged;
            GraphData.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(GraphData.SelectedModel): //선택된 item(model) 변경 시 알림
                        OnSelectedModelChanged(GraphData.SelectedModel);
                        break;
                    case nameof(GraphData.PageType):
                        PageType = GraphData.PageType;
                        break;
                    case nameof(GraphData.Yaxis_LabelVisible):
                        PlotModel.InvalidatePlot(false);
                        break;
                    case nameof(GraphData.Xaxis_LabelVisible):
                        OnXAxis_LabelChanged(GraphData.Xaxis_LabelVisible);
                        break;
                }
            };

        }

        private string XAxis_Title { get; } = "Time(s)";
        public LinearAxis XAxis { get; } = new LinearAxis();

        private ePAGE_TYPE pageType;
        private ePAGE_TYPE PageType
        {
            get
            {
                return pageType;
            }
            set
            {
                if (pageType != value)
                {
                    pageType = value;
                    SetAxisPosition(value);
                }
            }
        }



        private void SetDefaultAxis(PlotModel model)
        {
            XAxis.Position = AxisPosition.Bottom;
            XAxis.Title = XAxis_Title;

            XAxis.MajorGridlineStyle = LineStyle.Solid;
            XAxis.MinorGridlineStyle = LineStyle.Dot;
            XAxis.MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.Black);
            XAxis.MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.Black);
            XAxis.Zoom(0, 10);
            model.Axes.Add(XAxis);

        }

        private void UpdatePlotModel()
        {
            PlotModel.Series.Clear();
            double lastTime = 0.0;

            foreach (var graphModel in GraphCore.GraphData.Graphs)
            {
                var renderModel = graphModel.GraphRenderModel;
                PlotModel.Series.Add(renderModel.CurrentSeries);
                if (renderModel.LastPoint != null && lastTime < renderModel.LastPoint.X)
                    lastTime = graphModel.GraphRenderModel.LastPoint.X;
            }

            if (lastTime > 8.0)
            {
                XAxis.Zoom(lastTime - 8.0, lastTime + 2.0);
            }
            else
            {
                XAxis.Zoom(0, 10);
            }

            PlotModel.InvalidatePlot(true);
        }

        private void OnGraphCollectionChanged(object sender, EventArgs e)
        {
            //axis 추가
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(XAxis);
            foreach (var graph in GraphCore.GraphData.Graphs)
            {
                PlotModel.Axes.Add(graph.GraphRenderModel.YAxis);
            }
            SetAxisPosition(PageType);

            //data 추가
            PlotModel.Series.Clear();
            foreach(var graphModel in GraphCore.GraphData.Graphs)
            {
                var renderModel = graphModel.GraphRenderModel;
                PlotModel.Series.Add(renderModel.CurrentSeries);
            }

            PlotModel.InvalidatePlot(true);
        }

        private void OnSelectedModelChanged(GraphModel SelectedItem)
        {
            if (SelectedItem == null) return;
            //기존 item의 이벤트 제거 및 새 item 이벤트 추가
            if(CurrentItem.GraphRenderModel != null) CurrentItem.GraphRenderModel.PropertyChanged -= OnRenderModelChanged;
            CurrentItem = SelectedItem;
            CurrentItem.GraphRenderModel.PropertyChanged += OnRenderModelChanged;

            //xaxis와 selectedRow 의 색 맞춰주기
            ChangeAxisColor(XAxis, CurrentItem.GraphRenderModel.BaseColor);

            //page type 에 따른 view 분기처리
            SetAxisPosition(PageType);            
            PlotModel.InvalidatePlot(false);
        }

        private void OnRenderModelChanged(object sender, PropertyChangedEventArgs e) //render model => gridcontrol에서 뭔가 변경되는 경우를 감지하여 받아옴
        {
            if (CurrentItem != null)
            {
                switch (e.PropertyName)
                {
                    case nameof(GraphRenderModel.BaseColor):
                        {
                            var baseColor = CurrentItem.GraphRenderModel.BaseColor;
                            var yaxis = PlotModel.Axes.FirstOrDefault(x => x.Tag == CurrentItem.Tag);
                            if (yaxis != null) ChangeAxisColor((LinearAxis)yaxis, baseColor);
                            ChangeAxisColor(XAxis, baseColor);
                        }
                        break;
                    case nameof(GraphRenderModel.ValueType):
                        {
                            OnGraphCollectionChanged(null, EventArgs.Empty);
                        }
                        break;
                    case nameof(GraphRenderModel.Visible):
                        {
                            if(PageType == ePAGE_TYPE.SEPARATE_Y)
                            {
                                SetSepaYPage(); //separate의 경우 하나 안 보일때마다 YAxis 높이 다시계산
                            }
                            PlotModel.InvalidatePlot(true);
                        }
                        break;
                    case nameof(GraphRenderModel.PlotMode):
                        {
                            PlotModel.InvalidatePlot(true);
                        }
                        break;
                }
            }
        }

        private void ChangeAxisColor(LinearAxis axis, Color color)
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

            //plotmodel색도 맞춰줌
            PlotModel.PlotAreaBorderColor = oxyColor;
            PlotModel.InvalidatePlot(false);
            
        }

        private void SetAxisPosition(ePAGE_TYPE pageType)
        {
            switch (pageType)
            {
                case ePAGE_TYPE.SINGLE_Y:
                    SetSingleYPage();
                    break;
                case ePAGE_TYPE.MULTIPLE_Y:
                    SetMultiYPage();
                    break;
                case ePAGE_TYPE.SEPARATE_Y:
                    SetSepaYPage();
                    break;
            }
        }

       

        private void SetSingleYPage()
        {
            if(CurrentItem != null)
            {
                var yAxes = PlotModel.Axes.Where(x => x.Position == AxisPosition.Left);
                foreach (var axis in yAxes)
                {
                    resetAxis(axis);
                    if (axis.Tag == CurrentItem.Tag) axis.IsAxisVisible = true;
                    else axis.IsAxisVisible = false;
                }
                PlotModel.InvalidatePlot(false);
            }
            
        }

        private void SetMultiYPage()
        {
            if(CurrentItem != null)
            {
                int idx = 1;
                var yAxes = PlotModel.Axes.Where(x => x.Position == AxisPosition.Left);
                foreach(var axis in yAxes)
                {
                    resetAxis(axis);
                    if (axis.Tag == CurrentItem.Tag) axis.PositionTier = 0;
                    else
                    {
                        axis.PositionTier = idx++;
                    }
                        
                       
                }
                PlotModel.InvalidatePlot(false);
            }
        }

        private void SetSepaYPage()
        {
            if(CurrentItem != null)
            {
                var yaxes = PlotModel.Axes.Where(x => x.Position == AxisPosition.Left && x.IsAxisVisible == true).ToArray();
                int cnt = yaxes.Count();
                int reverseIdx = cnt;
                for(int i = 0; i < cnt; i++)
                {
                    resetAxis(yaxes[--reverseIdx]);
                    yaxes[reverseIdx].StartPosition = (double)i / cnt;
                    yaxes[reverseIdx].EndPosition = (double)(i + 1) / cnt;
                    
                }
                PlotModel.InvalidatePlot(false);
            }
        }

        private Axis resetAxis(Axis YAxis)
        {
            YAxis.IsAxisVisible = true;
            YAxis.PositionTier = 0;
            YAxis.StartPosition = 0;
            YAxis.EndPosition = 1;
            return YAxis;
        }

        private void OnXAxis_LabelChanged(bool isVisible)
        {
            if (isVisible)
            {
                XAxis.Title = XAxis_Title;
            }
            else
            {
                XAxis.Title = string.Empty;
            }
            PlotModel.InvalidatePlot(false);
        }
    }
}
