using OxyTest.Data;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OxyTest.Composition
{
	public class GraphCore
	{
		public Visual MainVisual { get; }

		public GraphData GraphData { get; }
		public GraphProcessor GraphProcessor { get; }
		public DialogService DialogService { get; }
		public PageNavigationService NavigationService { get; }

		public GraphCore(Visual visual)
		{
			MainVisual = visual;

			GraphData = new GraphData();
			GraphProcessor = new GraphProcessor(GraphData);
			DialogService = new DialogService(MainVisual);
			NavigationService = new PageNavigationService(this);
		}

		/// <summary>
		/// viewmodel에서 호출. view를 register하면 DispatcherTimer의 tick마다 model이 업데이트되었음을 받아올 수 있음.
		/// </summary>
		public void SubscribeModelUpdates(Action callback)
		{
			GraphProcessor.RegisterCallbackAction(callback);
		}

		public void UnSubscribeModelUpdates()
        {
			GraphProcessor.UnRegisterCallbackAction();
        }
	}
}
