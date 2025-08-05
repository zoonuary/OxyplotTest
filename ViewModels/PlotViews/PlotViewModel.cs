using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using OxyTest.Services;
using OxyTest.ViewModels.PlotViews.Internals;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace OxyTest.ViewModels
{
    public class PlotViewModel : ViewModelBase
    {
        private GraphCore GraphCore { get; }
        private GraphData GraphData { get; }
        public PlotModel PlotModel { get; }

        //internal classes
        public LayoutManager LayoutManager { get; }
        public CursorManager CursorManager { get; }
        public PlotControllerBuilder PlotControllerBuilder { get; }
        public PlotController PlotController { get; } //xaml에 바인딩됨

        private GraphModel CurrentItem = new GraphModel();

        public PlotViewModel(GraphCore graphCore)
        {
            GraphCore = graphCore;
            PlotModel = new PlotModel();
            
            SetDefaultAxis(PlotModel);
            LayoutManager = new LayoutManager(GraphCore);
            CursorManager = new CursorManager(PlotModel, XAxis);
            PlotControllerBuilder = new PlotControllerBuilder(GraphCore, CursorManager);
            PlotController = PlotControllerBuilder.SetController(PlotModel);

            //update 등록
            GraphCore.GraphProcessor.RegisterCallbackAction(UpdatePlotModel);


            //동기화 등록
            GraphCore.GraphProcessor.RegiscatCalbackAction_GraphSync(UpdateSync);

            GraphData = GraphCore.GraphData;
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
                    case nameof(GraphData.CursorType):
                        CursorType = GraphData.CursorType;
                        break;
                    case nameof(GraphData.Yaxis_LabelVisible):
                        PlotModel.InvalidatePlot(false);
                        break;
                    case nameof(GraphData.Xaxis_LabelVisible):
                        if (GraphData.Xaxis_LabelVisible)
                            XAxis.Title = XAxis_Title;
                        else
                            XAxis.Title = string.Empty;
                        PlotModel.InvalidatePlot(false);
                        break;
                    case nameof(GraphData.GridLineVisible):
                        if (GraphData.GridLineVisible)
                        {
                            XAxis.MajorGridlineStyle = LineStyle.Solid;
                            XAxis.MinorGridlineStyle = LineStyle.Dot;
                        }
                        else
                        {
                            XAxis.MajorGridlineStyle = LineStyle.None;
                            XAxis.MinorGridlineStyle = LineStyle.None;
                        }
                        PlotModel.InvalidatePlot(false);
                        break;
                    case nameof(GraphData.Xaxis_isFitMode):
                        if (GraphData.Xaxis_isFitMode)
                        {
                            UpdatePlotModel(GraphCore.GraphProcessor.LastEventTime);
                        }
                        break;
                }
            };

            XAxis.AxisChanged += (s, e) =>
            {
                foreach (var graph in GraphData.Graphs)
                {
                    graph.UpdatePlotData(XAxis);
                }

                

                //if (graphCore.GraphProcessor.eLOCAL_STATUS != eLOCAL_STATUS.LIVEUPDATE)
                //{

                //}
            };
        }

        private string XAxis_Title { get; } = "Time(s)";
        public LinearAxis XAxis { get; } = new LinearAxis { Key = Guid.NewGuid().ToString() };

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
                    LayoutManager.SetLayout(value, CurrentItem);
                    SetCursor();
                    PlotModel.InvalidatePlot(true);
                }
            }
        }

        private eCURSOR_TYPE cursorType;
        private eCURSOR_TYPE CursorType
        {
            get
            {
                return cursorType;
            }
            set
            {
                if(cursorType != value)
                {
                    cursorType = value;
                    SetCursor();
                }
            }
        }

        private void SetDefaultAxis(PlotModel model)
        {
            XAxis.Position = AxisPosition.Bottom;
            XAxis.Title = XAxis_Title;
            XAxis.Title = XAxis_Title;
            XAxis.MajorGridlineStyle = LineStyle.Solid;
            XAxis.MinorGridlineStyle = LineStyle.Dot;
            XAxis.MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.Black);
            XAxis.MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.Black);
            XAxis.Zoom(0, 10);

            var yaxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.Black),
                MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.Black)
            };
            yaxis.Zoom(0, 10);
            model.Axes.Add(XAxis);
            //model.Axes.Add(yaxis); //성능 문제로 제거
        }

        private void UpdatePlotModel(double lastTime)
        {
            PlotModel.Series.Clear();
            //double lastTime = 0.0;

            //foreach (var graphModel in GraphCore.GraphData.Graphs)
            //{
            //    var renderModel = graphModel.GraphRenderModel;
            //    PlotModel.Series.Add(renderModel.CurrentSeries);
            //    if (renderModel.LastPoint != null && lastTime < renderModel.LastPoint.X)
            //        lastTime = graphModel.GraphRenderModel.LastPoint.X;
            //}

            //get lasttime

            if (GraphData.Xaxis_isFitMode)
            {
                var range = GraphData.Xaxis_DefaultFitRange;
                if (lastTime <= range) 
                { 
                    XAxis.Zoom(0, lastTime);
                }
                else
                {
                    XAxis.Zoom(lastTime - range, lastTime);
                }
            }
            else
            {
                var range = GraphData.Xaxis_DefaultRange;
                var margin = GraphData.Xaxis_DefualtMargin;
                if(lastTime > (range - margin))
                {
                    XAxis.Zoom(lastTime - range, lastTime + margin);
                }
                else
                {
                    XAxis.Zoom(0, range);
                }
            }

            foreach (var graph in GraphData.Graphs)
            {
                graph.UpdatePlotData(XAxis);
                PlotModel.Series.Add(graph.GraphRenderModel.CurrentSeries);
            }

            PlotModel.InvalidatePlot(true);
        }

        private void UpdateSync(GraphSyncEventModel model)
        {
            if(model.Xaxis_ActualMaximum != XAxis.ActualMaximum
                && model.Xaxis_ActualMinimum != XAxis.ActualMinimum)
            {
                XAxis.Zoom(model.Xaxis_ActualMinimum, model.Xaxis_ActualMaximum);
                PlotModel.InvalidatePlot(false);
            }
        }

        private void OnGraphCollectionChanged(object sender, EventArgs e)
        {
            //axis 추가
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(XAxis);

            var Graphs = GraphCore.GraphData.Graphs;
            foreach (var graph in Graphs)
            {
                PlotModel.Axes.Add(graph.GraphRenderModel.YAxis);
            }
            LayoutManager.SetLayout(PageType, CurrentItem);

            //data 추가
            PlotModel.Series.Clear();
            foreach(var graphModel in Graphs)
            {
                var renderModel = graphModel.GraphRenderModel;
                PlotModel.Series.Add(renderModel.CurrentSeries);
            }

            CursorManager.OnGraphCollectionChanged(Graphs);
            SetCursor();

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
            LayoutManager.SetLayout(PageType, CurrentItem);
            PlotModel.InvalidatePlot(true);
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
                                LayoutManager.SetLayout(PageType, CurrentItem); //separate의 경우 하나 안 보일때마다 YAxis 높이 다시계산
                            }
                            PlotModel.InvalidatePlot(true);
                        }
                        break;
                    case nameof(GraphRenderModel.PlotMode):
                        {
                            PlotModel.InvalidatePlot(true);
                        }
                        break;
                    case nameof(GraphRenderModel.YAxis):
                        {
                            PlotModel.InvalidatePlot(false);
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

        private void SetCursor()
        {
            PlotModel.Annotations.Clear();
            switch (CursorType)
            {
                case eCURSOR_TYPE.DEFAULT:
                    break;
                case eCURSOR_TYPE.MEASUERMENT:
                    foreach (var cursor in CursorManager.GetMeasureLines(PageType))
                    {
                        PlotModel.Annotations.Add(cursor);
                    }
                    break;
                case eCURSOR_TYPE.DIFFERENCE:
                    foreach (var cursor in CursorManager.GetTargetLines(PageType))
                    {
                        PlotModel.Annotations.Add(cursor);
                    }
                    foreach (var cursor in CursorManager.GetPivotLines(PageType))
                    {
                        PlotModel.Annotations.Add(cursor);
                    }
                    break;
            }
            PlotModel.InvalidatePlot(false);
        }

    }
}
