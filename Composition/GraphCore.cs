using OxyTest.Data;
using OxyTest.Models.Event;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace OxyTest.Composition
{
	/*
	 * 전역적인 호출이 필요한 클래스를 모아둠. GraphCore만 알면 대부분의 기능을 수행 가능하도록
	 */
	public class GraphCore
	{
		public Visual MainVisual { get; }
		public Dispatcher Dispatcher { get; } 

		public GraphData GraphData { get; }
		public GraphProcessor GraphProcessor { get; }
		public DialogService DialogService { get; }

		public GraphCore(Visual visual)
		{
			MainVisual = visual;
			Dispatcher = MainVisual.Dispatcher;
			GraphData = new GraphData();
			GraphProcessor = new GraphProcessor(GraphData, Dispatcher);
			DialogService = new DialogService(MainVisual);
		}
	}
}
