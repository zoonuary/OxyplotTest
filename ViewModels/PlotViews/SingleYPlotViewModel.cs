using DevExpress.Mvvm;
using DevExpress.Xpf.WindowsUI.Navigation;
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
    public class SingleYPlotViewModel : ViewModelBase, INavigationAware
    {
        private GraphCore GraphCore { get; }

        public PlotModel PlotModel { get; }

        public SingleYPlotViewModel(GraphCore graphCore)
        {
            //set core
            GraphCore = graphCore;

            //plot model
            PlotModel = new PlotModel();
            SetDefaultAxis(PlotModel);

            //
            graphCore.GraphData.Graphs_CollectionChanged += OnGraphCollectionChanged;
            graphCore.GraphData.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(graphCore.GraphData.SelectedModel): //선택된 item(model) 변경 시 알림
                        OnSelectedModelChanged(graphCore.GraphData.SelectedModel);
                        break;
                }
            };
        }
        private GraphModel CurrentItem = new GraphModel();



        public LinearAxis XAxis { get; } = new LinearAxis();

        private void UpdatePlotModel()
        {
            PlotModel.Series.Clear();
            double lastTime = 0.0;
            
            foreach (var graphModel in GraphCore.GraphData.Graphs)
            {
                var renderModel = graphModel.GraphRenderModel;
                PlotModel.Series.Add(renderModel.CurrentSeries);
                if(renderModel.LastPoint != null && lastTime < renderModel.LastPoint.X)
                    lastTime = graphModel.GraphRenderModel.LastPoint.X;
            }

            if(lastTime > 8.0)
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
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(XAxis);
            foreach (var graph in GraphCore.GraphData.Graphs)
            {
                PlotModel.Axes.Add(AxisCopy(graph.GraphRenderModel.YAxis));
            }
            PlotModel.InvalidatePlot(false);
        }

        private void SetDefaultAxis(PlotModel model)
        {
            XAxis.Position = AxisPosition.Bottom;
            XAxis.Title = "Time (s)";

            XAxis.MajorGridlineStyle = LineStyle.Solid;
            XAxis.MinorGridlineStyle = LineStyle.Dot;
            XAxis.MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.Black);
            XAxis.MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.Black);
            model.Axes.Add(XAxis);
        }

        private void OnSelectedModelChanged(GraphModel SelectedItem)
        {
            if (SelectedItem == null) return;
            //기존 item의 이벤트 제거 및 새 item 이벤트 추가
            CurrentItem.PropertyChanged -= OnRenderModelChanged;
            CurrentItem = SelectedItem;
            CurrentItem.GraphRenderModel.PropertyChanged += OnRenderModelChanged;

            //xaxis와 selectedRow 의 색 맞춰주기
            ChangeAxisColor(XAxis, CurrentItem.GraphRenderModel.BaseColor);
            ShowSelectedAxis(CurrentItem.Tag);
            PlotModel.InvalidatePlot(false);
        }

        private void OnRenderModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CurrentItem != null && e.PropertyName == nameof(CurrentItem.GraphRenderModel.BaseColor))
            {
                Color baseColor = CurrentItem.GraphRenderModel.BaseColor;
                //plotmodel에서 yaxis를 찾아서 바꿈
                var yaxis = PlotModel.Axes.FirstOrDefault(x => x.Tag == CurrentItem.Tag);
                if (yaxis != null) ChangeAxisColor((LinearAxis)yaxis, baseColor);
                ChangeAxisColor(XAxis, baseColor);
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

        private void ShowSelectedAxis(object tag)
        {
            var yAxes = PlotModel.Axes.Where(x => x.Position == AxisPosition.Left);
            foreach (var axis in yAxes)
            {
                if (axis.Tag == tag) axis.IsAxisVisible = true;
                else axis.IsAxisVisible = false;
            }
        }

        private LinearAxis AxisCopy(LinearAxis axis)
        {
            return new LinearAxis
            {
                Position = axis.Position,
                Tag = axis.Tag,
                Key = axis.Key,
                Title = axis.Title,
                MajorGridlineStyle = axis.MajorGridlineStyle,
                MinorGridlineStyle = axis.MinorGridlineStyle,

                LabelFormatter = axis.LabelFormatter,

                AxislineColor = axis.AxislineColor,
                TicklineColor = axis.TicklineColor,
                TextColor = axis.TextColor,
                TitleColor = axis.TitleColor,
                MinorGridlineColor = axis.MinorGridlineColor,
                MajorGridlineColor = axis.MajorGridlineColor
            };
        }

        public void NavigatedTo(NavigationEventArgs e)
        {
            //Graph의 dataModel을 update하는 트리거 등록
            GraphCore.SubscribeModelUpdates(UpdatePlotModel);
        }

        public void NavigatingFrom(NavigatingEventArgs e)
        {
            GraphCore.UnSubscribeModelUpdates(); //더이상 series 이벤트 받지 않음
            PlotModel.Series.Clear(); //series clear
        }

        public void NavigatedFrom(NavigationEventArgs e)
        {

        }
    }
}
