using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OxyTest.Services
{
	public class GraphRenderLoop : IDisposable
	{
		private readonly Func<bool> CanRender;
		private readonly Action DoRender;
		private readonly DispatcherTimer Timer;
		private readonly EventHandler TickHandler;

		public GraphRenderLoop(TimeSpan interval, Func<bool> canRender, Action doRender)
		{
			CanRender = canRender;
			DoRender = doRender;
			Timer = new DispatcherTimer
			{
				Interval = interval
			};

			TickHandler = (s, e) =>
			{
				if (canRender())
				{
					DoRender?.Invoke();
				}
			};

			Timer.Tick += TickHandler;
		}

		public void Start() => Timer.Start();

		public void Stop() => Timer.Stop();

		public void Dispose()
		{
			Timer.Stop();
			Timer.Tick -= TickHandler;
		}
	}
}
