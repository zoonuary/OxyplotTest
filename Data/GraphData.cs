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
	/*
	 * Graph가 보여지기 위해 필요한 공용 데이터 모음
	 * 1. Graphs = 사용자가 추가한 signal 정보 저장, play시 파형 데이터가 추가되는 GraphModel클래스의 모음 
	 * 2. pageType = single/ multiple/ separeate 3가지 view type을 보여주기 위해 추가된 enum변수
	 * 3. cursorType = default / measurement / difference 3가지 커서로 값 탐색을 위해 추가된 enum 변수
	 * 4. gridlinevisible = plot area에서 격자선 추가 / 삭제 변수. 렌더링 성능에 크게 영향을 미치는 요소 중 하나인 bool 변수
	 * 5. selectedmodel = 현재 선택된 graphmodel을 나타냄. plot에서의 강조, gridcontrol에서의 선택을 도움
	 * 6. xaxis_... = x축 관련 설정 내용들은 모든 Graph가 공유함으로 별도의 동기화함수를 쓰는 대신, property로 빼놓음. 추후 더 많은 양의 property가 필요해지는경우 class로 분리
	 */
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
			CursorType = eCURSOR_TYPE.DEFAULT;
			Yaxis_LabelVisible = true;
			Xaxis_LabelVisible = true;
			GridLineVisible = true;

			//
			xaxis_isFitMode = false;
			xaxis_isSyncMode = false;

			Xaxis_DefaultRange = 10.0;
			Xaxis_DefualtMargin = 2.0;
			Xaxis_DefaultFitRange = 300.0;
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

		private eCURSOR_TYPE cursorType;
		public eCURSOR_TYPE CursorType
        {
			get => cursorType;
            set
            {
				if(cursorType != value)
                {
					cursorType = value;
					foreach(var graph in Graphs)
                    {
						graph.clearCursorValues();
                    }
					NotifyPropertyChanged(nameof(CursorType));
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

		private bool xaxis_isFitMode;
		public bool Xaxis_isFitMode
        {
			get => xaxis_isFitMode;
            set
            {
				if(xaxis_isFitMode != value)
                {
					xaxis_isFitMode = value;
					NotifyPropertyChanged(nameof(xaxis_isFitMode));
                }
            }
        }

		private bool xaxis_isSyncMode;
		public bool Xaxis_isSyncMode
        {
			get => xaxis_isSyncMode;
            set
            {
				if(xaxis_isSyncMode != value)
                {
					xaxis_isSyncMode = value;
					NotifyPropertyChanged(nameof(xaxis_isSyncMode));
				}
            }
        }


		public double Xaxis_DefaultRange { get; private set; } //fit기능이 눌려지지 않았을때 기본적으로 보여줄 xaxis 의 총 길이(초)

		public double Xaxis_DefualtMargin { get; private set; } // fit기능이 눌려지지 않았을때, Graph의 마지막 시간과 xaxis의 마지막 시간 차이(0 인 경우, 그래프가 plotarea 위 마지막까지 그려짐)

		public double Xaxis_DefaultFitRange { get; private set; } //기본 300초, Fit 시 5분 기준으로 보여줌 

		private bool gridLineVisible;
		public bool GridLineVisible
		{
			get => gridLineVisible;
            set
            {
				if(gridLineVisible != value)
                {
					gridLineVisible = value;
					foreach (var graph in Graphs)
					{
						graph.GraphRenderModel.SetGridLineVisible(value);
					}
					NotifyPropertyChanged(nameof(GridLineVisible));
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

		public void OnDBCChanged(List<string> DBCnames)
        {
			graphs.RemoveAll(x => !DBCnames.Contains(x.SignalDataModel.ReferencedDBC));
			Graphs_CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

		public EventHandler? Graphs_CollectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          