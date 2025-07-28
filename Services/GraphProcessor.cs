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

		private Action callbackAction { get; set; }

		private void OnEvent(EventModel model)
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
					break;
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

		/// <summary>
		/// model 을 순회하며 rendermodel로 데이터를 밀어넣음. 이후 viewmodel들에게 model이 변경되었으니 view를 갱신하도록 전파
		/// </summary>
		private void UpdateGraph()
		{
			foreach(var model in GraphData.Graphs)
			{
				if (LastEventTime < (defaultMaxTime - defaultOffsetTime))
					model.UpdatePlotData(defaultMiTime, defaultMaxTime);
				else
					model.UpdatePlotData(rangeMinValue, rangeMaxValue);
			}
			callbackAction?.Invoke(); //등록된 action 실행 => viewmodel로 update되었음을 알려줌
		}

		public void RegisterCallbackAction(Action callback)
		{
			callbackAction = callback;
		}

		public void UnRegisterCallbackAction()
        {
			callbackAction = null;
        }
	}
}
