using HPT.Common.Models;
using HPT.Common.Utils;
using OxyTest.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace OxyTest.Events
{
	public class EventListener
	{
		private static EventListener instance;
		public static EventListener Instance
		{
			get
			{
				if (instance == null)
					instance = new EventListener();
				return instance;
			}
		}

		//register 중복 방지 플래그
		public bool isRegistered = false;

		/// <summary>
		/// 프로그램 시작 이벤트
		/// </summary>
		public event EventHandler<EventArgs> Start;

		/// <summary>
		/// 프로그램 종료 이벤트
		/// </summary>
		public event EventHandler<EventArgs> Stop;

		/// <summary>
		/// 버스 이벤트 데이터 수신 이벤트
		/// </summary>
		public event EventHandler<BusEventReceivedEventArgs> OnEventData;

		/// <summary>
		/// DBC 업데이트 이벤트
		/// </summary>
		public event EventHandler<EventArgs> DbcUpdated;

		/// <summary>
		/// HWSetting 업데이트 이벤트
		/// </summary>
		public event EventHandler<EventArgs> HWSettingChanged;

		public void RegisterListeners()
		{
			if (isRegistered) return;
			isRegistered = true;

			//============================== 이벤트 수신 스레드와 parsing 스레드 분리
			
			CancellationTokenSource tokenSource = new CancellationTokenSource();//비동기 루프 정리용 TokenSource 생성															
			_ = LocalBroadCaster.StartBroadCastServiceAsync(tokenSource.Token);//Event 비동기 처리 루프 시작
			AppDomain.CurrentDomain.ProcessExit += (s, e) => { tokenSource.Cancel(); };//루프 종료 트리거 등록


			//============================== 수신 이벤트 등록
			EventHandlerManager.Default.Add<EventArgs>("Start", (s, e) => Start?.Invoke(s, e));
			EventHandlerManager.Default.Add<EventArgs>("Stop", (s, e) => Stop?.Invoke(s, e));
			EventHandlerManager.Default.Add<BusEventReceivedEventArgs>("OnEventData", (s, e) => OnEventData?.Invoke(s, e));
			EventHandlerManager.Default.Add<EventArgs>("DbcUpdated", (s, e) => DbcUpdated?.Invoke(s, e));


			//============================== 수신 이벤트 처리
			Start += (s, e) =>
			{
				LocalBroadCaster.EnqueueEvent(new EventModel(eEVENT_BEHAVIOR_TYPE.START));
			};

			Stop += (s, e) =>
			{
				LocalBroadCaster.EnqueueEvent(new EventModel(eEVENT_BEHAVIOR_TYPE.STOP));
			};

			OnEventData += (s, e) =>
			{
				EventData eventData = e.EventData;
				switch (eventData.Type)
				{
					case EventType.CAN_ERR_MESSAGE:
					case EventType.CAN_MESSAGE:
						if(eventData is CanDataFrame canEventData)
						{
							var message = new EventModel(eEVENT_BEHAVIOR_TYPE.ONEVENT, eventData.Type, canEventData.MsgId, canEventData.DLC, canEventData.Data, canEventData.TimeStamp, canEventData.IsExtended);
							LocalBroadCaster.EnqueueEvent(message);
						}
						break;

					case EventType.CAN_FD_ERR_MESSAGE:
					case EventType.CAN_FD_MESSAGE:
						if (eventData is CanFdDataFrame canFDEventData)
						{
							//Models.StoredMessage message = new Models.StoredMessage(ePROTOCOL_TYPE.CANFD, canFDEventData.MsgId, canFDEventData.DLC, canFDEventData.Data, canFDEventData.TimeStamp, canFDEventData.IsExtended);
							//GraphEventBroadCaster.AddStoredCANFDMessage(message);
							//GraphEventBroadCaster.BroadCast(new GraphEventData { eventType = eGRAPH_EVENT_TYPE.ONEVENT, data = message });
						}
						break;
				}
			};

			DbcUpdated += (s, e) =>
			{


			};
		}

	}
}
