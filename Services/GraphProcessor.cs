using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OxyTest.Services
{
	public class GraphProcessor
	{
		
		private readonly Dictionary<Type, Action<object>> EventHandlerMap;
		

		public GraphProcessor(GraphData graphData, Dispatcher dispatcher, DataSlotManager dataSlotManager)
		{
			GraphData = graphData;
			Dispatcher = dispatcher;
			DataSlotManager = dataSlotManager;
			EventHandlerMap = new Dictionary<Type, Action<object>>
			{
				{typeof(EventModel), param => OnEvent_HandleEventModel((EventModel)param)},
				{typeof(GraphSyncEventModel), param => OnEvent_HandleGraphSync((GraphSyncEventModel)param)}
			};

			// BroadCast 이벤트 구독
			LocalBroadCaster.Subscribe(OnEvent);
												
			RenderLoop = new GraphRenderLoop(
				RenderSpan, //render timing
				() => true, //condition
				() => UpdateGraph() //action
				);

		}

		#region Properties
		private GraphData GraphData { get; }
		private Dispatcher Dispatcher { get; }
		private DataSlotManager DataSlotManager { get; }

		private GraphRenderLoop RenderLoop { get; } //UI Thread에서 돌리는 loop. renderSpan주기로 호출됨
		private TimeSpan RenderSpan { get; } = TimeSpan.FromMilliseconds(200);
		public double LastEventTime { get; set; } = 0.0;

		//callbacks
		private List<Action<eLOCAL_STATUS>> CallbackActions_LocalStatusChanged { get; set; } = new List<Action<eLOCAL_STATUS>>();
		private Action<double> CallbackAction_Update { get; set; }
		private Action<GraphSyncEventModel> callbackAction_GraphSync { get; set; }


		private eLOCAL_STATUS local_status = eLOCAL_STATUS.STOPPED;
		public eLOCAL_STATUS eLOCAL_STATUS
		{
			get => local_status;
			set
			{
				if (local_status != value)
				{
					local_status = value;
					switch (local_status)
					{
						case eLOCAL_STATUS.LIVEUPDATE:
							RenderLoop.Start();
							break;
						case eLOCAL_STATUS.PAUSED:
							RenderLoop.Stop();
							break;
						case eLOCAL_STATUS.STOPPED:
							RenderLoop.Stop();
							break;
					}
				}

				foreach(var callback in CallbackActions_LocalStatusChanged)
                {
					callback?.Invoke(local_status);
                }
			}
		}

		#endregion

		#region Methods
		private void OnEvent(object model)
		{
			//데이터 클래스 타입 파싱 최적화 : hash lookup
			if (EventHandlerMap.TryGetValue(model.GetType(), out var handler))
			{
				handler(model);
			}
		}

		private void AppendEventData(EventModel eventModel)
		{
			foreach (var graphModel in GraphData.Graphs)
			{
				if (graphModel.SignalDataModel.ID == eventModel.ID)
				{
					graphModel.AppendData(eventModel);
				}
			}

			LastEventTime = eventModel.TimeStamp;
		}

		private void ClearData()
		{
			foreach (var model in GraphData.Graphs)
			{
				model.ClearData();
			}
			LastEventTime = 0.0;
			CallbackAction_Update?.Invoke(LastEventTime);
		}

		private void UpdateGraph()
		{
			CallbackAction_Update?.Invoke(LastEventTime);
		}

		public void RegisterCallbackAction_LocalStatusChanged(Action<eLOCAL_STATUS> callback)
        {
			CallbackActions_LocalStatusChanged.Add(callback);
        }

		public void RegisterCallbackAction(Action<double> callback)
		{
			CallbackAction_Update = callback;
		}

		public void RegiscatCalbackAction_GraphSync(Action<GraphSyncEventModel> callback)
		{
			callbackAction_GraphSync = callback;
		}

		public void UnRegisterCallbackAction()
		{
			CallbackAction_Update = null;
		}
		#endregion

		#region SubMethods
		private void OnEvent_HandleEventModel(EventModel model)
		{
			switch (model.BehaviorType)
			{
				case eEVENT_BEHAVIOR_TYPE.START: //start => 수신된 이벤트를 지속적으로 update
					eLOCAL_STATUS = eLOCAL_STATUS.LIVEUPDATE;
					LastEventTime = 0.0;
					break;
				case eEVENT_BEHAVIOR_TYPE.STOP: //udate 중지
					eLOCAL_STATUS = eLOCAL_STATUS.STOPPED;
					break;
				case eEVENT_BEHAVIOR_TYPE.ONEVENT:
					AppendEventData(model);
					break;
				case eEVENT_BEHAVIOR_TYPE.DBC_UPDATED:
					Dispatcher.BeginInvoke(new Action(() =>
					{
						GraphData.OnDBCChanged(LocalBroadCaster.GetReferencedDBCNames());
					}));
					break;
				case eEVENT_BEHAVIOR_TYPE.CLEAR:
					ClearData();
					break;
			}
		}

		private void OnEvent_HandleGraphSync(GraphSyncEventModel model)
		{
			if (GraphData.InstanceID != model.InstanceID            //내가 보낸게 아닌 메시지만
				&& eLOCAL_STATUS != eLOCAL_STATUS.LIVEUPDATE    //Plot이 계속해서 갱신되지 않는 시점에만
				&& GraphData.Xaxis_isSyncMode)                  //sync mode인 경우
			{
				callbackAction_GraphSync?.Invoke(model);
			}
		}
		#endregion
	}
}
