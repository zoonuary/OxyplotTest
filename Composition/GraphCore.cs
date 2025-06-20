using OxyTest.Data;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Composition
{
	public class GraphCore
	{
		public GraphData GraphData { get; }
		public GraphProcessor GraphProcessor { get; }
		public DialogService DialogService { get; }
		public PageNavigationService NavigationService { get; }
		public GraphCore()
		{
			GraphData = new GraphData();
			GraphProcessor = new GraphProcessor(GraphData);
			DialogService = new DialogService();
			NavigationService = new PageNavigationService(this);
		}

		/// <summary>
		/// winform에서 호출되길 기대하는 dll 임으로, winform을 호출한 elementhost의 handle을 필요한 class들에 넣어주는 함수
		/// </summary>
		/// <param name="hwnd">winform handle</param>
		public void InitializeWithHandle(IntPtr hwnd)
		{
			DialogService.Init(hwnd);
		}

		/// <summary>
		/// viewmodel에서 호출. view를 register하면 DispatcherTimer의 tick마다 model이 업데이트되었음을 받아올 수 있음.
		/// </summary>
		public void SubscribeModelUpdates(Action callback)
		{
			GraphProcessor.RegisterCalbackAction(callback);
		}
	}
}
