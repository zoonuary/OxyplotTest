using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyTest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Services
{
    public class PlotControllerBuilder
    {
        private GraphData GraphData;
        private PlotController PlotController { get; }
        private static IViewCommand<OxyMouseDownEventArgs> multiAxisPanManipulateCommand { get; set; }

        public PlotModel PlotModel { get; set; }
        public LineAnnotation MeasurementCursor { get; set; }
        public LineAnnotation TargetCursor { get; set; }
        public LineAnnotation PivotCursor { get; set; }
        public PlotControllerBuilder(GraphData graphData)
        {
            GraphData = graphData;

            PlotController = new PlotController();
            multiAxisPanManipulateCommand = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) => 
            {
                ctrl.AddMouseManipulator(view, new MultiAxisPanManipulator(view, GraphData.PageType), args);
                //if (GraphData.PageType == ePAGE_TYPE.MULTIPLE_Y)
                //{
                //    ctrl.AddMouseManipulator(view, new MultiAxisPanManipulator(view), args);
                //}
                //else
                //{
                //    ctrl.AddMouseManipulator(view, new PanManipulator(view), args);
                //}
            });

            GraphData.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.Equals(nameof(GraphData.CursorType))) 
                {
                    OnCursorTypeChanged(GraphData.CursorType);
                }
            };
        }

        public PlotController SetController(PlotModel model, LineAnnotation measurementCursor, LineAnnotation pivotCursor, LineAnnotation targetCursor)
        {
            PlotModel = model;
            MeasurementCursor = measurementCursor;
            PivotCursor = pivotCursor;
            TargetCursor = targetCursor;

            //PlotModel.Annotations.Add(MeasurementCursor);
            //PlotModel.Annotations.Add(PivotCursor);
            //PlotModel.Annotations.Add(TargetCursor);

            PlotController.UnbindAll();
            PlotController.BindMouseDown(OxyMouseButton.Right, PlotCommands.HoverTrack);
            PlotController.BindMouseDown(OxyMouseButton.Left, multiAxisPanManipulateCommand); //multiAxis인 경우 axis positiontier가 높은 axis가 아니라, 이미 세팅된 axis를 계속해서 움직이던 문제를 해결하기위한 커스텀 커맨드
            PlotController.BindMouseWheel(PlotCommands.ZoomWheel);
            PlotController.BindMouseDown(OxyMouseButton.Middle, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) => { HandelMiddleButtonDown(args); }));
            PlotController.BindMouseDown(OxyMouseButton.Middle, OxyModifierKeys.Control, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, ctrl, args) => { HandleCtrlMiddleButtonDown(args); }));
            return PlotController;
        }

        public void OnMeasureCursorMove(double xPosition)
        {
            if(GraphData.CursorType == eCURSOR_TYPE.MEASUERMENT
                && MeasurementCursor != null)
            {
                MeasurementCursor.X = PlotModel.DefaultXAxis.InverseTransform(xPosition);
                PlotModel.InvalidatePlot(false); //화면 갱신
            }
        }

        private void OnCursorTypeChanged(eCURSOR_TYPE type)
        {
            switch (type)
            {
                case eCURSOR_TYPE.DEFAULT:
                    SetDefaultCursor();
                    break;
                case eCURSOR_TYPE.DIFFERENCE:
                    SetDifferenceCursor();
                    break;
                case eCURSOR_TYPE.MEASUERMENT:
                    SetMeasurementCursor();
                    break;
            }
        }

        private void SetDefaultCursor()
        {
            PlotModel.Annotations.Clear();
        }

        private void SetMeasurementCursor()
        {
            PlotModel.Annotations.Clear();
            PlotModel.Annotations.Add(MeasurementCursor);
        }

        private void SetDifferenceCursor()
        {
            PlotModel.Annotations.Clear();
            PlotModel.Annotations.Add(TargetCursor);
            PlotModel.Annotations.Add(PivotCursor);
        }

        private void HandelMiddleButtonDown(OxyMouseDownEventArgs args)
        {
            if (GraphData.CursorType == eCURSOR_TYPE.DIFFERENCE)
            {
                TargetCursor.X = PlotModel.DefaultXAxis.InverseTransform(args.Position.X);
            }
            PlotModel.InvalidatePlot(false);
            args.Handled = true;
        }
        
        private void HandleCtrlMiddleButtonDown(OxyMouseDownEventArgs args)
        {
            if(GraphData.CursorType == eCURSOR_TYPE.DIFFERENCE)
            {
                PivotCursor.X = PlotModel.DefaultXAxis.InverseTransform(args.Position.X);
            }
            PlotModel.InvalidatePlot(false);
            args.Handled = true;
        }
    }


    public class MultiAxisPanManipulator : MouseManipulator
    {
        private IPlotView View { get; }
        private ePAGE_TYPE PageType { get; }
        public MultiAxisPanManipulator(IPlotView view, ePAGE_TYPE pageType) : base(view)
        {
            View = view;
            PageType = pageType;
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

            if (this.XAxis != null)
            {
                this.XAxis.Pan(this.PreviousPosition, e.Position);
            }

            if (this.YAxis != null)
            {
                this.YAxis.Pan(this.PreviousPosition, e.Position);
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
                || position.Y > plotArea.Bottom
                || PageType != ePAGE_TYPE.MULTIPLE_Y)
            {
                base.AssignAxes(position);
            }
            else
            {
                foreach (var axis in this.PlotView.ActualModel.Axes)
                {
                    if (axis.IsHorizontal()) this.XAxis = axis;
                    if (axis.IsVertical() && axis.PositionTier == 0) this.YAxis = axis;
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
}
