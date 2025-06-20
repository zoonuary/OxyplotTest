using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Utils
{
	public class BoundedQueue<T> : IEnumerable<T>
	{
		private readonly Queue<T> queue = new Queue<T>();
		private readonly object _lock = new object();
		private int max;
		private int current;

		public BoundedQueue(int maxCount)
		{
			max = maxCount;
			current = 0;
		}

		public void Enqueue(T item)
		{
			lock (_lock)
			{
				queue.Enqueue(item);
				current++;
				while(current > max)
				{
					queue.Dequeue();
					current--;
				}
			}
		}

		public bool TryDequeue(out T item)
		{
			lock (_lock)
			{
				if(queue.Count > 0 && current > 0)
				{
					item = queue.Dequeue();
					current--;
					return true;
				}
				item = default;
				return false;
			}
		}

		public void SetMaxCount(int maxCount)
		{
			lock (_lock)
			{
				max = maxCount;
				while(current > max)
				{
					queue.Dequeue();
					current--;
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock (_lock)
			{
				return new List<T>(queue).GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
