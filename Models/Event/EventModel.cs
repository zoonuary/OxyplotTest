using HPT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Event
{
	public class EventModel
	{
		public EventModel(eEVENT_BEHAVIOR_TYPE behaviorType)
		{
			BehaviorType = behaviorType;
		}

		public EventModel(eEVENT_BEHAVIOR_TYPE behaviorType, EventType type, uint id, byte dlc, byte[] data, ulong timestamp, bool isExtended)
		{
			BehaviorType = behaviorType;
			Type = type;
			ID = IsExtended ? (id & 0x1FFFFFFF) : id;
			DLC = dlc;
			Data = data;

			TimeStamp = Math.Floor((double)(timestamp * 1e-9) * 1e6) / 1e6;
			//double timeSec = Math.Floor((double)(timestamp * 1e-9) * 1e6) / 1e6; //소숫점 아래 9자리까지 계산, 6자리까지 자름
			//TimeStamp = DateTime.MinValue.AddSeconds(timeSec);
			//TimeStamp = new DateTime((long)(timestamp / 100));
			IsExtended = SetData(type);
		}
		



		private bool SetData(EventType type)
		{
			var newData = new byte[DLC_Table[DLC]];

			if (DLC > 0)
				System.Buffer.BlockCopy(Data, 0, newData, 0, DLC_Table[DLC]);
			Data = newData;

			switch (type)
			{
				case EventType.CAN_ERR_MESSAGE:
				case EventType.CAN_MESSAGE:

					if (DLC >= 0 && DLC <= 8)
						return true;
					else
						return false;

				case EventType.CAN_FD_ERR_MESSAGE:
				case EventType.CAN_FD_MESSAGE:
					if (DLC >= 0 && DLC <= 15)
						return true;
					else
						return false;

			}
			return false;
		}

		public static readonly byte[] DLC_Table = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };
		public eEVENT_BEHAVIOR_TYPE BehaviorType { get; }
		public EventType Type { get; }
		public uint ID { get; }
		public byte DLC { get; }
		public byte[] Data { get; set; }
		//public DateTime TimeStamp { get; } => oxyplot에서 요구하는 timestamp가 double형태임으로 그에 맞춰 변경
		public double TimeStamp { get; }
		
		public bool IsExtended { get; }
		public bool IsValidMessage { get; }
	}
}


