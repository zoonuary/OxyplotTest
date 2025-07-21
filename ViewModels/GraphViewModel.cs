using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Models.Graph;
using OxyTest.Services;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace OxyTest.ViewModels
{
    public class GraphViewModel : ViewModelBase
    {
        public GraphCore GraphCore { get; }
        private GridViewModel GridViewModel { get; } //GraphView에 종속되는 Gridveiw의 Viewmodel. 일종의 부모 자식 관계로 취급
        

        public GraphViewModel(GraphCore graphCore, GridViewModel gridViewModel)
        {
            GraphCore = graphCore;
            GridViewModel = gridViewModel;

            CMD_START = new DelegateCommand(() => GraphCore.GraphProcessor.StartProcess());
            CMD_STOP = new DelegateCommand(() => GraphCore.GraphProcessor.StopProcess());
            //----------------------------------------
            CMD_AppendGraph = new DelegateCommand(OpenSignalAddDialog);
            CMD_RemoveGraph = new DelegateCommand(OnSignalsRemoved);
            CMD_ClearGraph = new DelegateCommand(OnSignalsCleared);
            CMD_ShowRawValues = new DelegateCommand(OnShowRawValues);
            CMD_ShowPhysicalValues = new DelegateCommand(OnShowPhysicalValues);
            

            //----------------------------------------
            CMD_ViewTypeChanged = new DelegateCommand<object>(OnViewTypeChanged);
        }

        public ICommand CMD_START { get; }
        public ICommand CMD_STOP { get; }

        //---------------------------------------
        public ICommand CMD_AppendGraph { get; }
        public ICommand CMD_RemoveGraph { get; }
        public ICommand CMD_ClearGraph { get; }
        public ICommand CMD_ShowRawValues { get; }
        public ICommand CMD_ShowPhysicalValues { get; }

        //---------------------------------------
        public ICommand CMD_ViewTypeChanged { get; }


        public void OpenSignalAddDialog()
        {
            GraphCore.DialogService.ShowSignalAddDialog(OnSignalAdded);
        }

        private void OnSignalAdded(GraphModel model)
        {
            GraphCore.GraphData.AddGraph(model);
        }

        private void OnSignalsRemoved() //일부 삭제(selectedItems)
        {
            foreach (var graph in GridViewModel.SelectedItems.Cast<GraphModel>().ToList())
            {
                if (graph is GraphModel model)
                {
                    GraphCore.GraphData.RemoveGraph(model);
                }
            }
        }

        private void OnSignalsCleared() //전체 삭제
        {
            GraphCore.GraphData.ClearGraphs();
        }

        private void OnShowRawValues() //이 raw / physical 두 속성은 렌더링 성능에 영향이 갈 수도 있음(model이 많아지거나, signal series 갯수가 많은 경우) 
        {
            foreach(var graph in GraphCore.GraphData.Graphs)
            {
                graph.ValueType = eVALUE_TYPE.RAW; //property 변경할 때마다 값이 다른경우 화면이 갱신됨으로 주의 필요
            }
        }

        private void OnShowPhysicalValues()
        {
            foreach(var graph in GraphCore.GraphData.Graphs)
            {
                graph.ValueType = eVALUE_TYPE.PHYSICAL;
            }
        }

        private void OnViewTypeChanged(object param)
        {
            if (param is string s && Enum.TryParse<ePAGE_TYPE>(s, out var viewType))
            {
                GraphCore.GraphData.PageType = viewType;
            }
        }
        
    }
}
