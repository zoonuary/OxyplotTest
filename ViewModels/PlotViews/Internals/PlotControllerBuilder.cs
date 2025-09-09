using OxyPlot;
using OxyPlot.Axes;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OxyTest.ViewModels.PlotViews.Internals
{
    public class PlotControllerBuilder
    {
        private GraphData GraphData { get; }
        private CursorManager CursorManager { get; }
        private PlotController PlotController { get; }
        private IViewCommand<OxyMouseDownEventArgs> multiAxisPanManipulateCommand { get; set; }
        private IViewCommand<OxyMouseDownEventArgs> ShowTracker { get; set; }
        private IViewCommand<OxyMouseWheelEventArgs> ZoomWheel { get; set; }
        public PlotModel PlotModel { get; set; }
        public ViewPortInfo ViewPortInfo { get; set; }
        private PlotPopupHelper PlotPopupHelper { get; }

        public PlotControllerBuilder(GraphCore graphCore, CursorManager cursorManager)
        {
            GraphData = graphCore.GraphData;
            CursorManager = cursorManager;

            PlotController = new PlotController();
            PlotPopupHelper = new PlotPopupHelper();

            multiAxisPanManipulateCommand = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) =>
            {
                ctrl.AddMouseManipulator(view, new MultiAxisPanManipulator(view, GraphData), args);
            });

            ShowTracker = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) =>
            {
                ctrl.AddMouseManipulator(view, new SignalTrackerManipulator(view, PlotPopupHelper, GraphData), args);
            });

            ZoomWheel = new DelegatePlotCommand<OxyMouseWheelEventArgs>((view, ctrl, args) =>
            {
                double factor = 1;
                var manipulator = new ZoomStepManipulator(view, GraphData.PageType, GraphData, graphCore.GraphData.eLOCAL_STATUS, graphCore.GraphData.InstanceID) { Step = args.Delta * 0.001 * factor };
                manipulator.Started(args);
                SetViewPortInfo(view.ActualModel.Axes.FirstOrDefault(axis => axis.IsHorizontal()));
            });
        }

        public PlotController SetController(PlotModel model)
        {
            PlotModel = model;
            PlotController.UnbindAll();
            //PlotController.BindMouseDown(OxyMouseButton.Right, PlotCommands.HoverTrack);
            PlotController.BindMouseDown(OxyMouseButton.Right, ShowTracker);

            PlotController.BindMouseDown(OxyMouseButton.Left, multiAxisPanManipulateCommand); //multiAxis인 경우 axis positiontier가 높은 axis가 아니라, 이미 세팅된 axis를 계속해서 움직이던 문제를 해결하기위한 커스텀 커맨드
            PlotController.BindMouseWheel(ZoomWheel);
            PlotController.BindMouseDown(OxyMouseButton.Middle, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) => { HandelMiddleButtonDown(args); }));
            PlotController.BindMouseDown(OxyMouseButton.Middle, OxyModifierKeys.Control, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) => { HandleCtrlMiddleButtonDown(args); }));
            return PlotController;
        }

        public void SetViewPortInfo(Axis? xaxis)
        {
            if (PlotModel == null || xaxis == null)
            {
                ViewPortInfo = new ViewPortInfo(0, 0, 0, new OxyRect());
                return;
            }

            double min = xaxis.ActualMinimum;
            double max = xaxis.ActualMaximum;

            var area = PlotModel.PlotArea;
            int dip = (int)Math.Round(area.Width);

            ViewPortInfo = new ViewPortInfo(min, max, dip, area);
        }

        public void OnMeasureCursorMove(double xPosition)
        {
            if (GraphData.CursorType == eCURSOR_TYPE.MEASUERMENT)
            {
                double transformedX = PlotModel.DefaultXAxis.InverseTransform(xPosition);
                CursorManager.SetMeasureX(transformedX);
                foreach (var model in GraphData.Graphs)
                {
                    model.YTime = transformedX;
                    model.Y = model.GetNearestValue(transformedX);
                }
                PlotModel.InvalidatePlot(false); //화면 갱신
            }
        }

        private void HandelMiddleButtonDown(OxyMouseDownEventArgs args)
        {
            if (GraphData.CursorType == eCURSOR_TYPE.DIFFERENCE)
            {
                double transformedX = PlotModel.DefaultXAxis.InverseTransform(args.Position.X);
                CursorManager.SetTargetX(transformedX);
                foreach (var model in GraphData.Graphs)
                {
                    model.YTime = transformedX;
                    model.Y = model.GetNearestValue(transformedX);

                    double pivotX = CursorManager.PivotXPosition;
                    model.dYTime = model.YTime - pivotX;

                    model.dY = model.Y - model.GetNearestValue(pivotX);
                }
            }
            PlotModel.InvalidatePlot(false);
            args.Handled = true;
        }

        private void HandleCtrlMiddleButtonDown(OxyMouseDownEventArgs args)
        {
            if (GraphData.CursorType == eCURSOR_TYPE.DIFFERENCE)
            {
                double transformedX = PlotModel.DefaultXAxis.InverseTransform(args.Position.X);
                CursorManager.SetPivotX(transformedX);

                foreach (var model in GraphData.Graphs)
                {
                    double pivotX = CursorManager.PivotXPosition;
                    model.dYTime = model.YTime - pivotX;
                    model.dY = model.Y - model.GetNearestValue(pivotX);
                }
            }
            PlotModel.InvalidatePlot(false);
            args.Handled = true;
        }
    }

    public class MultiAxisPanManipulator : MouseManipulator
    {
        private IPlotView View { get; }
        private ePAGE_TYPE PageType { get; }
        private bool IsSyncMode { get; }
        private eLOCAL_STATUS LocalStatus { get; }
        private object InstanceID { get; }
        private GraphData GraphData { get; }
        //public MultiAxisPanManipulator(IPlotView view, ePAGE_TYPE pageType, bool isSyncMode, eLOCAL_STATUS localstatus, object instanceID) : base(view)
        //{
        //    View = view;
        //    PageType = pageType;
        //    IsSyncMode = isSyncMode;
        //    LocalStatus = localstatus;
        //    InstanceID = instanceID;
        //}

        public MultiAxisPanManipulator(IPlotView view, GraphData graphData) : base(view)
        {
            View = view;
            GraphData = graphData;
            PageType = graphData.PageType;
            IsSyncMode = graphData.Xaxis_isSyncMode;
            LocalStatus = graphData.eLOCAL_STATUS;
            InstanceID = graphData.InstanceID;
        }


        private ScreenPoint PreviousPosition { get; set; }

        private bool IsPanEnabled { get; set; }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            if (!this.IsPanEnabled)
            {
                return;
            }

            this.View.SetCursorType(CursorType.Default);
            e.Handled = true;
        }


        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            if (!this.IsPanEnabled)
            {
                return;
            }

            if (this.XAxis != null
                && LocalStatus != eLOCAL_STATUS.LIVEUPDATE)
            {
                this.XAxis.Pan(this.PreviousPosition, e.Position);
                if (IsSyncMode)
                {
                    var eventData = new GraphSyncEventModel(InstanceID, XAxis.ActualMinimum, XAxis.ActualMaximum);
                    LocalBroadCaster.Instance.BroadCast(eventData);
                }
            }

            if (this.YAxis != null)
            {
                this.YAxis.Pan(this.PreviousPosition, e.Position);
                if (PageType == ePAGE_TYPE.SINGLE_Y)
                {
                    foreach (var axis in PlotView.ActualModel.Axes)
                    {
                        if (axis.IsVertical())
                        {
                            //axis.Pan(this.PreviousPosition, e.Position);
                            axis.Zoom(YAxis.ActualMinimum, YAxis.ActualMaximum);
                        }
                    }
                }

            }

            this.PlotView.InvalidatePlot(false);
            this.PreviousPosition = e.Position;
            e.Handled = true;
        }

        public override void Started(OxyMouseEventArgs e)
        {
            var position = e.Position;
            this.StartPosition = position;
            this.PreviousPosition = position;

            var plotArea = this.PlotView.ActualModel.PlotArea;

            if (position.X < plotArea.Left
                || position.X > plotArea.Right
                || position.Y < plotArea.Top
                || position.Y > plotArea.Bottom)
            {
                base.AssignAxes(position);
            }
            else
            {
                if (PageType == ePAGE_TYPE.MULTIPLE_Y)
                {
                    foreach (var axis in this.PlotView.ActualModel.Axes)
                    {
                        if (axis.IsHorizontal())
                        {
                            this.XAxis = axis;
                        }

                        if (axis.IsVertical() && axis.PositionTier == 0)
                        {
                            this.YAxis = axis;
                            break;
                        }
                    }
                }
                else
                {
                    base.AssignAxes(position);
                }
            }

            this.IsPanEnabled = (this.XAxis != null && this.XAxis.IsPanEnabled)
                                || (this.YAxis != null && this.YAxis.IsPanEnabled);

            if (this.IsPanEnabled)
            {
                this.View.SetCursorType(CursorType.Pan);
                e.Handled = true;
            }
        }
    }
    public class ZoomStepManipulator : MouseManipulator
    {
        private bool IsSyncMode { get; }
        private object InstanceID { get; }
        private eLOCAL_STATUS LocalStatus { get; }
        private double Maxrange { get; }

        public ZoomStepManipulator(IPlotView plotView, ePAGE_TYPE pageType, GraphData graphData, eLOCAL_STATUS localstatus, object instanceid) : base(plotView)
        {
            PageType = pageType;
            IsSyncMode = graphData.Xaxis_isSyncMode;
            Maxrange = graphData.DataChunkSize;
            LocalStatus = localstatus;
            InstanceID = instanceid;
        }



        public bool FineControl { get; set; }
        public double Step { get; set; }
        private ePAGE_TYPE PageType { get; }

        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);

            //yaxis가 여러개인경우, position tier가 바뀌어도 조작되는 y축은 바뀌지 않음으로, 새로 찾아옴
            var position = e.Position;
            //this.StartPosition = position;
            //this.PreviousPosition = position;

            var plotArea = this.PlotView.ActualModel.PlotArea;
            if (position.X < plotArea.Left
                || position.X > plotArea.Right
                || position.Y < plotArea.Top
                || position.Y > plotArea.Bottom)
            {
                base.AssignAxes(position);
            }
            else
            {
                if (PageType == ePAGE_TYPE.MULTIPLE_Y)
                {
                    foreach (var axis in this.PlotView.ActualModel.Axes)
                    {
                        if (axis.IsVertical() && axis.PositionTier == 0)
                        {
                            this.YAxis = axis;
                            break;
                        }
                    }
                }
            }

            if (!((this.XAxis != null && this.XAxis.IsZoomEnabled) || (this.YAxis != null && this.YAxis.IsZoomEnabled)))
            {
                return;
            }

            var current = this.InverseTransform(e.Position.X, e.Position.Y);
            if (Step > 0)
            {
                Step = 1 + Step;
            }
            else
            {
                Step = 1.0 / (1 - Step);
            }

            if (this.XAxis != null
                && LocalStatus != eLOCAL_STATUS.LIVEUPDATE)
            {

                if (((XAxis.ActualMaximum - XAxis.ActualMinimum) / Step) <= Maxrange)
                {
                    this.XAxis.ZoomAt(Step, current.X);
                }
                else
                {
                    var min = XAxis.ActualMinimum;
                    this.XAxis.Zoom(min, min + Maxrange);
                }
            }

            if (this.YAxis != null)
            {
                this.YAxis.ZoomAt(Step, current.Y);
                if (PageType == ePAGE_TYPE.SINGLE_Y)
                {
                    foreach (var axis in PlotView.ActualModel.Axes)
                    {
                        if (axis.IsVertical())
                        {
                            axis.Zoom(this.YAxis.ActualMinimum, this.YAxis.ActualMaximum);
                        }
                    }
                }
            }

            //if (this.YAxis != null)
            //{
            //    this.YAxis.Pan(this.PreviousPosition, e.Position);
            //    if (PageType == ePAGE_TYPE.SINGLE_Y)
            //    {
            //        foreach (var axis in PlotView.ActualModel.Axes)
            //        {
            //            if (axis.IsVertical())
            //            {
            //                //axis.Pan(this.PreviousPosition, e.Position);
            //                axis.Zoom(YAxis.ActualMinimum, YAxis.ActualMaximum);
            //            }
            //        }
            //    }

            //}


            if (IsSyncMode)
            {
                var eventData = new GraphSyncEventModel(InstanceID, XAxis.ActualMinimum, XAxis.ActualMaximum);
                LocalBroadCaster.Instance.BroadCast(eventData);
            }

            this.PlotView.InvalidatePlot(false);
            e.Handled = true;
        }


    }

    public class SignalTrackerManipulator : MouseManipulator
    {
        private PlotPopupHelper PlotPopupHelper { get; }
        private GraphData GraphData { get; }

        public SignalTrackerManipulator(IPlotView plotView, PlotPopupHelper plotPopupHelper, GraphData graphData) : base(plotView)
        {
            PlotPopupHelper = plotPopupHelper;
            GraphData = graphData;
        }

        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);
            Delta(e);
        }

        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            e.Handled = true;

            var plotView = base.PlotView as FrameworkElement;
            if (plotView == null)
            {
                return;
            }

            if (PlotView.ActualModel == null)
            {
                return;
            }

            if (!PlotView.ActualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
            {
                return;
            }

            var screenPoint = plotView.PointToScreen(new Point(e.Position.X, e.Position.Y));
            List<CursorData> data = new List<CursorData>();
            var XPoint = base.PlotView.ActualModel.DefaultXAxis.InverseTransform(e.Position.X);
            var XData = new CursorData("X", XPoint.ToString());
            data.Add(XData);

            foreach (var model in GraphData.Graphs)
            {
                var value = model.GetNearestValue(XPoint);
                if (value != null)
                {
                    var name = model.SignalDataModel.Name.ToString();
                    data.Add(new CursorData(name, value.ToString(), model.GraphRenderModel.BaseColor));
                }
            }

            PlotPopupHelper.SetPopupData(data);
            PlotPopupHelper.OpenPopup(screenPoint);
        }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            e.Handled = true;
            PlotPopupHelper.ClosePopup();
        }

    }
}
