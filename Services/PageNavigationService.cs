using DevExpress.Xpf.WindowsUI;
using DevExpress.Xpf.WindowsUI.Navigation;
using OxyTest.Composition;
using OxyTest.ViewModels;
using OxyTest.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OxyTest.Services
{


	public class PageNavigationService
	{
		private GraphCore GraphCore { get; }

		public PageNavigationService(GraphCore graphCore)
		{
			GraphCore = graphCore;
			navigationFrame = new NavigationFrame { AnimationType = AnimationType.None };
			Initialize_Views();
		}

		private void Initialize_Views()
		{
			SingleYPlotView single = new SingleYPlotView()
			{
				DataContext = new SingleYPlotViewModel(GraphCore)
			};

			MultipleYPlotView multy = new MultipleYPlotView()
			{
				DataContext = new MultipleYPlotViewModel(GraphCore)
			};

			SeparateYPlotView separate = new SeparateYPlotView()
			{
				DataContext = new SeparateYPlotViewModel(GraphCore)
			};

			CachedPages.Add(ePAGE_TYPE.SINGLE_Y, single);
			CachedPages.Add(ePAGE_TYPE.MULTIPLE_Y, multy);
			CachedPages.Add(ePAGE_TYPE.SEPARATE_Y, separate);
		}

		private readonly Dictionary<ePAGE_TYPE, UserControl> CachedPages = new Dictionary<ePAGE_TYPE, UserControl>();

		private NavigationFrame navigationFrame { get; }

		public NavigationFrame NavigationFrame => navigationFrame;

		public void Navigate(ePAGE_TYPE pagetype)
		{
			switch (pagetype)
			{
				case ePAGE_TYPE.SINGLE_Y:
					{
						if (CachedPages.ContainsKey(ePAGE_TYPE.SINGLE_Y))
						{
							navigationFrame.Navigate(CachedPages[ePAGE_TYPE.SINGLE_Y]);
						}
					}
					break;
				case ePAGE_TYPE.MULTIPLE_Y:
                    {
                        if (CachedPages.ContainsKey(ePAGE_TYPE.MULTIPLE_Y))
                        {
							navigationFrame.Navigate(CachedPages[ePAGE_TYPE.MULTIPLE_Y]);
                        }
                    }
					break;
				case ePAGE_TYPE.SEPARATE_Y:
                    {
                        if (CachedPages.ContainsKey(ePAGE_TYPE.SEPARATE_Y))
                        {
							navigationFrame.Navigate(CachedPages[ePAGE_TYPE.SEPARATE_Y]);
                        }
                    }
					break;
			}
		}
	}
}
