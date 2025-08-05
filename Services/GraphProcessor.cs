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
		private GraphData GraphData { get; }

		private Dispatcher Dispatcher { get; }

		private readonly Dictionary<Type, Action<object>> EventHandlerMap;

		public object InstanceID { get; } = Guid.NewGuid();

		public GraphProcessor(GraphData graphData, Dispatcher dispatcher)
		{
			GraphData = graphData;
			Dispatcher = dispatcher;
			EventHandlerMap = new Dictionary<Type, Action<object>>
			{
				{ typeof(EventModel),  param => OnEvent_HandleEventModel((EventModel)param)},
				{typeof(GraphSyncEventModel), param => OnEvent_HandleGraphSync((GraphSyncEventModel)param) }
			};

			// BroadCast 이벤트 구독
			LocalBroadCaster.Subscribe(OnEvent);
												
			RenderLoop = new GraphRenderLoop(
				RenderSpan, //render timing
				() => true, //condition
				() => UpdateGraph() //action
				);
		}

		private GraphRenderLoop RenderLoop { get; }

		//OnEvent로 들어오는 CAN Message들을 임시로 저장하는 queue (소모하여 Graph render)
		//private ConcurrentQueue<EventModel> StagedMessages = new ConcurrentQueue<EventModel>();
		private TimeSpan RenderSpan { get; } = TimeSpan.FromMilliseconds(200);

		private Action<double> callbackAction { get; set; }

		private Action<GraphSyncEventModel> callbackAction_GraphSync { get; set; } //필요한 action이 더 많아질 경우, dictionary<string , Action<object>> 로 추상화 할 것

		private void OnEvent(object model)
		{
			//데이터 클래스 타입 파싱 최적화 : hash lookup
			if(EventHandlerMap.TryGetValue(model.GetType(), out var handler))
            {
				handler(model);
            }
		}

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
			if(InstanceID != model.InstanceID					//내가 보낸게 아닌 메시지만
				&& eLOCAL_STATUS != eLOCAL_STATUS.LIVEUPDATE	//Plot이 계속해서 갱신되지 않는 시점에만
				&& GraphData.Xaxis_isSyncMode)					//sync mode인 경우
            {
				callbackAction_GraphSync?.Invoke(model);
            }
        }

		private eLOCAL_STATUS local_status = eLOCAL_STATUS.STOPPED;
		public eLOCAL_STATUS eLOCAL_STATUS
        {
			get => local_status;
			set
            {
				if(local_status != value)
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
            }
        }

		public double LastEventTime { get; set; } = 0.0;

		private double defaultMiTime = 0.0;
		private double defaultMaxTime = 10.0;
		private double defaultOffsetTime = 2.0;
		private double rangeMinValue => LastEventTime - defaultMaxTime; //minvalue 좀 넓게 조정
		private double rangeMaxValue => LastEventTime + defaultOffsetTime;
		private void AppendEventData(EventModel eventModel)
		{
			foreach(var graphModel in GraphData.Graphs)
			{
				if(graphModel.SignalDataModel.ID == eventModel.ID)
				{
					graphModel.AppendData(eventModel);
				}
			}
			LastEventTime = eventModel.TimeStamp;
		}

		private void ClearData()
        {
			foreach(var model in GraphData.Graphs)
            {
				model.ClearData();
			}
			LastEventTime = 0.0;
			callbackAction?.Invoke(LastEventTime);
        }


		/// <summary>
		/// model 을 순회하며 rendermodel로 데이터를 밀어넣음. 이후 viewmodel들에게 model이 변경되었으니 view를 갱신하도록 전파
		/// </summary>
		private void UpdateGraph()
		{
			//foreach(var model in GraphData.Graphs)
			//{
   //             if (GraphData.Xaxis_isFitMode)
   //             {

   //             }
   //             else
   //             {

   //             }

			//	if (LastEventTime < (defaultMaxTime - defaultOffsetTime))
			//		model.UpdatePlotData(defaultMiTime, defaultMaxTime);
			//	else
			//		model.UpdatePlotData(rangeMinValue, rangeMaxValue);
			//}
			callbackAction?.Invoke(LastEventTime); //등록된 action 실행 => viewmodel로 update되었음을 알려줌
		}

		public void RegisterCallbackAction(Action<double> callback)
		{
			callbackAction = callback;
		}

		public void RegiscatCalbackAction_GraphSync(Action<GraphSyncEventModel> callback)
        {
			callbackAction_GraphSync = callback;
        }

		public void UnRegisterCallbackAction()
        {
			callbackAction = null;
        }
	}
}
