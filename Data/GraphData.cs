                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     ﻿using OxyTest.Models.Graph;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
	public class GraphData
	{
		private readonly object _lock = new object();
		public GraphData()
		{
			Initialize();
		}

		private void Initialize()
        {
			pageType = ePAGE_TYPE.SINGLE_Y;
			Yaxis_LabelVisible = true;
			Xaxis_LabelVisible = true;
        }

		private ePAGE_TYPE pageType;
		public ePAGE_TYPE PageType
        {
			get => pageType;
            set
            {
				if(pageType != value)
                {
					pageType = value;
					NotifyPropertyChanged(nameof(PageType));
                }
            }
        }

		private bool yaxis_labelVisible;
		public bool Yaxis_LabelVisible
        {
			get => yaxis_labelVisible;
            set
            {
				if(yaxis_labelVisible != value)
                {
					yaxis_labelVisible = value;
					foreach(var graph in Graphs)
                    {
						graph.GraphRenderModel.IsTitleVisible = value;
					}
					NotifyPropertyChanged(nameof(Yaxis_LabelVisible));
                }
            }
        }
		private bool xaxis_labelVisible;
		public bool Xaxis_LabelVisible
        {
			get => xaxis_labelVisible;
            set
            {
				if(xaxis_labelVisible != value)
                {
					xaxis_labelVisible = value;
					NotifyPropertyChanged(nameof(Xaxis_LabelVisible));
                }
            }
        }

		private GraphModel selectedModel;
		public GraphModel SelectedModel
		{
			get => selectedModel;
			set
			{
				if(selectedModel != value)
				{
					if(selectedModel != null) selectedModel.Selected = false;
					selectedModel = value;
					if (value != null) value.Selected = true;
					NotifyPropertyChanged(nameof(SelectedModel));
				}
			}
		}


		private readonly List<GraphModel> graphs = new List<GraphModel>();
		public List<GraphModel> Graphs
		{
			get
			{
				lock (_lock)
					return graphs.ToList(); //헷갈림 방지 주석. list자체는 복사본이지만, data인 개별 graphmodel은 원본임.
			}
		}

		public void AddGraph(GraphModel model)
		{
			model.GraphRenderModel.IsTitleVisible = Yaxis_LabelVisible; // 나중에 Yaxis_LabelVisible 말고도 추가로 GraphData클래스에서 반영을 책임저야할 프로퍼티가 생기면, 별도의 initialize함수 추가.
			lock (_lock)
			{
				graphs.Add(model);
			}
			Graphs_CollectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool RemoveGraph(GraphModel model)
		{
			bool result = false;
			lock (_lock)
			{
				result = graphs.Remove(model);
			}
			if (result)
			{
				Graphs_CollectionChanged?.Invoke(this, EventArgs.Empty);
			}
			return result;
		}

		public void ClearGraphs()
		{
			lock (_lock)
			{
				graphs.Clear();
			}
			Graphs_CollectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public EventHandler? Graphs_CollectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          