using OxyTest.Data;
using OxyTest.Events;
using OxyTest.Models.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Services
{
	public class GraphProcessor
	{
		private GraphData GraphData { get; }

		public GraphProcessor(GraphData graphData)
		{
			GraphData = graphData;

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

		private readonly List<Action> callbackActions = new List<Action>();

		private void OnEvent(EventModel model)
		{
			switch (model.BehaviorType)
			{
				case eEVENT_BEHAVIOR_TYPE.START: //start => 수신된 이벤트를 지속적으로 update
					RenderLoop.Start();
					break;
				case eEVENT_BEHAVIOR_TYPE.STOP: //udate 중지
					RenderLoop.Stop();
					break;
				case eEVENT_BEHAVIOR_TYPE.ONEVENT:
					AppendEventData(model);
					break;
				case eEVENT_BEHAVIOR_TYPE.DBC_UPDATED:
					break;
			}
		}

		private void AppendEventData(EventModel eventModel)
		{
			foreach(var graphModel in GraphData.Graphs)
			{
				if(graphModel.SignalDataModel.ID == eventModel.ID)
				{
					graphModel.AppendData(eventModel);
				}
			}
		}

		/// <summary>
		/// model 을 순회하며 rendermodel로 데이터를 밀어넣음. 이후 viewmodel들에게 model이 변경되었으니 view를 갱신하도록 전파
		/// </summary>
		private void UpdateGraph()
		{
			foreach(var model in GraphData.Graphs)
			{
				model.PushListToRenderModel();
			}
			
			//등록된 action 실행 => viewmodel로 update되었음을 알려줌
			foreach(var action in callbackActions)
			{
				action?.Invoke();
			}
		}

		public void RegisterCalbackAction(Action callback)
		{
			callbackActions.Add(callback);
		}

		public void StartProcess()
		{
			RenderLoop.Start();
		}

		public void StopProcess()
		{
			RenderLoop.Stop();
		}
	}
}
