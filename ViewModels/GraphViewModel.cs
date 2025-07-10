using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Models.Graph;
using OxyTest.Services;
using System.Windows.Input;
using System.Windows.Threading;

namespace OxyTest.ViewModels
{
	public class GraphViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }
		public GraphViewModel(GraphCore graphCore)
		{
			GraphCore = graphCore;

			PlotModel = new PlotModel();


			var axis = new DateTimeAxis
			{
				Position = AxisPosition.Bottom,
				StringFormat = "HH:mm:ss.fff",
			};
			PlotModel.Axes.Add(axis);

			var yaxis2 = new LinearAxis
			{
				Position = AxisPosition.Left,
				//Key = "Y2",
				//PositionTier = 1,
				//StartPosition = 0.5,
				//EndPosition = 1.0
			};
			PlotModel.Axes.Add(yaxis2);

			CMD_START = new DelegateCommand(() => GraphCore.GraphProcessor.StartProcess());
			CMD_STOP = new DelegateCommand(() => GraphCore.GraphProcessor.StopProcess());
			CMD_AppendGraph = new DelegateCommand(OpenSignalAddDialog);
		}

		public ICommand CMD_START { get; }
		public ICommand CMD_STOP { get; }
		public ICommand CMD_AppendGraph { get; }

		//임시, view에 들어갈 plotModel
		public PlotModel PlotModel { get; }



		public void OpenSignalAddDialog()
		{
			GraphCore.DialogService.ShowSignalAddDialog(OnSignalAdded);
		}

		private void OnSignalAdded(GraphModel model)
		{
			GraphCore.GraphData.AddGraph(model);
			//GraphCore.GraphData.Graphs.Add(model);


			//아래 주석은 추가로 graph를 그릴 때 필요한 property를 커스텀해서 추가하던 코드.




			//var graphModel = new GraphModel(this) //모델 생성
			//{
			//	DataMode = eDATA_MODE.PHYSICAL,

			//	//graph datas
			//	//Data = analogWaveform.Clone(),
			//	PhysicalData = analogWaveform.Clone(),
			//	RawData = analogWaveform.Clone(),

			//	Tag = Guid.NewGuid(),
			//	Selected = false,
			//	GridLineVisible = this.GridLineVisible,
			//	Visible = true,
			//	ForeColor = WebColors.GetColorTemplate(),
			//	XRange = XRange, //추가안하면 시간 동기화가 안되어있는것처럼 보임 
			//					 //YRange = YRange
			//};
			////Range 설정
			//graphModel.PhysicalRange = graphModel.GetPhysicalRange(signal.Length, signal.Factor, signal.Offset, UnsignedType);
			//graphModel.RawRange = graphModel.GetRawRange(signal.Length, UnsignedType);
			////Data default 설정
			////graphModel.Data = graphModel.IsRawmode ? graphModel.RawData : graphModel.PhysicalData;
			////YAxis Label Value Converter 설정
			//graphModel.YAxis.SetConverter(item.ValueDescriptions);


			////해당 model이 can인지 can fd인지 알 수 있는 방법은?
			//UpdateGraph(GraphEventBroadCaster.GetStoredCANMessage(), graphModel);
			//UpdateGraph(GraphEventBroadCaster.GetStoredCANFDMessage(), graphModel);

			//Graphs.Add(graphModel);
			//SelectedRow = graphModel;
		}

	}
}
