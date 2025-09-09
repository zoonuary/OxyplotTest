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
		public GraphProcessor(GraphData graphData)
		{
			GraphData = graphData;
			RenderLoop = new GraphRenderLoop(
				RenderSpan, //render timing
				() => true, //condition
				() => UpdateGraph() //action
				);

			GraphData.PropertyChanged += (s, e) =>
			{
				if(e.PropertyName == nameof(GraphData.eLOCAL_STATUS))
                {
                    switch (GraphData.eLOCAL_STATUS)
                    {
						case eLOCAL_STATUS.LIVEUPDATE:
							RenderLoop.Start();
							break;
						case eLOCAL_STATUS.PAUSED:
						case eLOCAL_STATUS.STOPPED:
							RenderLoop.Stop();
							break;
					}
                }
			};
		}

		private GraphData GraphData { get; }
		public GraphRenderLoop RenderLoop { get; } //UI Thread에서 돌리는 loop. renderSpan주기로 호출됨
		private TimeSpan RenderSpan { get; } = TimeSpan.FromMilliseconds(200);


		private Action CallbackAction_Render { get; set; }
		public void RegisterCallbackAction_Render(Action callback)
		{
			CallbackAction_Render = callback;
		}

		private void UpdateGraph()
		{
			CallbackAction_Render?.Invoke();
		}

		//private eLOCAL_STATUS local_status = eLOCAL_STATUS.STOPPED;
		//public eLOCAL_STATUS eLOCAL_STATUS.
		//{
		//	get => local_status;
		//	set
		//	{
		//		if (local_status != value)
		//		{
		//			local_status = value;
		//			switch (local_status)
		//			{
		//				case eLOCAL_STATUS.LIVEUPDATE:
		//					RenderLoop.Start();
		//					break;
		//				case eLOCAL_STATUS.PAUSED:
		//					RenderLoop.Stop();
		//					break;
		//				case eLOCAL_STATUS.STOPPED:
		//					RenderLoop.Stop();
		//					break;
		//			}
		//		}

		//		foreach(var callback in CallbackActions_LocalStatusChanged)
		//              {
		//			callback?.Invoke(local_status);
		//              }
		//	}
		//}


		//private void OnEvent_HandleEventModel(EventModel model)
		//{
		//	switch (model.BehaviorType)
		//	{
		//		case eEVENT_BEHAVIOR_TYPE.START: //start => 수신된 이벤트를 지속적으로 update
		//			eLOCAL_STATUS = eLOCAL_STATUS.LIVEUPDATE;
		//			LastEventTime = 0.0;
		//			break;
		//		case eEVENT_BEHAVIOR_TYPE.STOP: //udate 중지
		//			eLOCAL_STATUS = eLOCAL_STATUS.STOPPED;
		//			break;
		//		case eEVENT_BEHAVIOR_TYPE.ONEVENT:
		//			AppendEventData(model);
		//			break;
		//		case eEVENT_BEHAVIOR_TYPE.DBC_UPDATED:
		//			Dispatcher.BeginInvoke(new Action(() =>
		//			{
		//				GraphData.OnDBCChanged(LocalBroadCaster.Instance.GetReferencedDBCNames());
		//			}));
		//			break;
		//		case eEVENT_BEHAVIOR_TYPE.CLEAR:
		//			ClearData();
		//			break;
		//	}
		//}

	}
}
