using HPT.Common.Models;
using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OxyTest.Services
{
    public class GraphEventHandler
    {
        private readonly Dictionary<Type, Action<object>> LiveEventHandlerMap;

		private GraphData GraphData { get; }
		private Dispatcher Dispatcher { get; }

		private Action<GraphSyncEventModel> CallbackAction_GraphSync { get; set; }

		private Action CallbackAction_Clear { get; set; }

		public GraphEventHandler(GraphData graphData,  Dispatcher dispatcher)
        {
			GraphData = graphData;
			Dispatcher = dispatcher;


            LocalBroadCaster.Instance.Subscribe(OnEvent);

            LiveEventHandlerMap = new Dictionary<Type, Action<object>>
            {
                {typeof(EventModel), param => OnEvent_HandleEventModel((EventModel)param)},
                {typeof(GraphSyncEventModel), param => OnEvent_HandleGraphSync((GraphSyncEventModel)param)}
            };

        }
		public void RegisterCallbackAction_GraphSync(Action<GraphSyncEventModel> callback)
		{
			CallbackAction_GraphSync = callback;
		}



		public void OnEvent(object model)
        {
			if (model == null) new ArgumentNullException(nameof(model));

            //데이터 클래스 타입 파싱 최적화 : hash lookup
            if (LiveEventHandlerMap.TryGetValue(model.GetType(), out var handler))
            {
                handler(model);
            }
            //throw new NotSupportedException($"[GraphEventHandler] Invalid Type to parse");
        }


		private void OnEvent_HandleEventModel(EventModel model)
		{
			switch (model.BehaviorType)
			{
				case eEVENT_BEHAVIOR_TYPE.START: //start => 수신된 이벤트를 지속적으로 update
					GraphData.eLOCAL_STATUS = eLOCAL_STATUS.LIVEUPDATE;
					GraphData.LastEventTime = 0.0;
					break;
				case eEVENT_BEHAVIOR_TYPE.STOP: //udate 중지
					GraphData.eLOCAL_STATUS = eLOCAL_STATUS.STOPPED;
					break;
				case eEVENT_BEHAVIOR_TYPE.ONEVENT:
					AppendEventData(model);
					break;
				case eEVENT_BEHAVIOR_TYPE.DBC_UPDATED:
					Dispatcher.BeginInvoke(new Action(() =>
					{
						GraphData.OnDBCChanged(LocalBroadCaster.Instance.GetReferencedDBCNames());
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
				&& GraphData.eLOCAL_STATUS != eLOCAL_STATUS.LIVEUPDATE    //Plot이 계속해서 갱신되지 않는 시점에만
				&& GraphData.Xaxis_isSyncMode)                  //sync mode인 경우
			{
				CallbackAction_GraphSync?.Invoke(model);
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
			GraphData.LastEventTime = eventModel.TimeStamp;
		}

		private void ClearData()
		{
			foreach (var model in GraphData.Graphs)
			{
				model.ClearData();
			}
			GraphData.LastEventTime = 0.0;
			CallbackAction_Clear?.Invoke();
		}

		public void RegisterCallbackAction_Clear(Action callback)
		{
			CallbackAction_Clear = callback;
		}
	}
}
