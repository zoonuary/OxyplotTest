using OxyTest.Models.Event;
using OxyTest.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OxyTest.Events
{
	public static class LocalBroadCaster
	{
		//broadcast 전 수신된 이벤트를 쌓아두는 queue

		/*
		 *  이벤트 broadcast 전, 수신된 메시지의 임시 저장 queue
		 */
		private static readonly ConcurrentQueue<EventModel> StagedMessages = new ConcurrentQueue<EventModel>();

		/*
		 *  이벤트 broadcast 후, 메시지 local 저장 queue(기본 10000개)
		 */
		private static BoundedQueue<EventModel> bufferedMessages = new BoundedQueue<EventModel>(10000);

		/*
		 * 이 broadcaster를 구독중인 Graph view instances
		 */
		private static readonly List<Action<object>> subscribers = new List<Action<object>>();

		/*
		 * 참조중인 dbc 이름
		 */
		private static List<string> referencedDBCNames = new List<string>();

		private static readonly SemaphoreSlim semaphoreSignal = new(0);
		
		private static object _lock = new object();

		//graph instance가 이벤트 구독, 구독 시 EventListener로 받아온 메시지를 지속적으로 수신
		public static void Subscribe(Action<object> handler)
		{
			if (!EventListener.Instance.isRegistered)
				EventListener.Instance.RegisterListeners();

			lock (_lock)
			{
				if (!subscribers.Contains(handler))
				{
					subscribers.Add(handler);
				}
			}
		}

		//이벤트 구독 취소
		public static void UsSubscribe(Action<object> handler)
		{
			lock (_lock)
			{
				subscribers.Remove(handler);
			}
		}

		//이벤트를 queue에 추가 및 semaphore로 queue 추가 알림.
		public static void EnqueueEvent(EventModel model)
		{
			StagedMessages.Enqueue(model);
			semaphoreSignal.Release(); //worker, Task StartBroadCastServiceAsync에게 알려줌
		}

		//semaphore를 통한 알림을 받기 전까지 대기상태, 알림을 받으면 dequeue하여 데이터 처리
		public static Task StartBroadCastServiceAsync(CancellationToken token)
		{
			return Task.Run(async () =>
			{
				try
				{
					while (true)
					{
						await semaphoreSignal.WaitAsync(token);
						if (StagedMessages.TryDequeue(out var message))
						{
							// 처리 로직 실행
							BroadCast(message);
						}
					}
				}
				catch (OperationCanceledException) { }//루프 종료 시 동작 수행
			});
		}

		//Buffer(FIFOqueue, 수신된 메시지 임시 보관 장소, 기본 10000개)에 수신된 메시지 저장
		public static void EnqueueMessageToBuffer(EventModel model)
		{
			lock (_lock)
			{
				bufferedMessages.Enqueue(model);
			}
		}

		//이벤트 모델을 구독 중인 Graph instance들에게 object 타입 parameter를 전달함
		//graphprocessor가 가지는 dictionary에 존재하는 클래스라면 뭐든 파싱 가능함
		public static void BroadCast(object model)
		{
			List<Action<object>> copy = new List<Action<object>>();
			lock (_lock)
			{
				copy = subscribers.ToList();
			}
			foreach(var handler in copy)
			{
				try
				{
					handler(model);
				}
				catch (Exception) { }
			}
		}

		public static Queue<EventModel> GetBufferedMessages()
		{
			Queue<EventModel> queue;
			lock (_lock)
            {
				queue = new Queue<EventModel>(bufferedMessages);
			}
			return queue;
		}

		public static void ClearBufferedMessages()
		{
            lock (_lock)
            {
				while (bufferedMessages.TryDequeue(out _)) ;
			}
		}

        public static List<string> GetReferencedDBCNames()
        {
            lock (_lock)
            {
                return new List<string>(referencedDBCNames);
            }
        }

        public static void SetReferencedDBCNames(List<string> dbcNames)
        {
            lock (_lock)
            {
				referencedDBCNames.Clear();
				referencedDBCNames.AddRange(dbcNames);
            }
        }
    }
}
