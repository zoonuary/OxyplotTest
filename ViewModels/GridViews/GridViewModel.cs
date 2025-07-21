using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Models.Graph;
using OxyTest.ViewModels.GridViews.Internals;
using OxyTest.ViewModels.GridVIews.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace OxyTest.ViewModels
{
	public class GridViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }
		public GraphData GraphData { get; }
		public GridControl GridControl { get; set; }

		public GridViewModel(GraphCore graphCore)
		{
			//데이터 초기 설정
			GraphCore = graphCore;
			GraphData = GraphCore.GraphData;

			//Graphs => view 에 바인딩되는 모델 컬렉션. 
			Graphs = new ObservableCollection<GraphModel>(GraphData.Graphs);

			//commands
			CMD_OpenColorPickerDialog = new DelegateCommand<object>(OnOpenColorPickerDialog);



			//
			Initialize_RegisterEvents();
			Initialize_GridveiwSetting();

			//combobox용 collection 세팅
			ValueTypes = new ObservableCollection<GridValueType>
			{
				new GridValueType("Raw", eVALUE_TYPE.RAW),
				new GridValueType("Physical", eVALUE_TYPE.PHYSICAL)
			};

			PlotTypes = new ObservableCollection<GridPlotType> //
			{
				new GridPlotType("Line", ePLOT_MODE.LINE),
				new GridPlotType("Point", ePLOT_MODE.POINT),
				new GridPlotType("Line + Point", ePLOT_MODE.LINE_POINT)
			};
		}

		private void Initialize_GridveiwSetting()
        {
			//컬럼 초기 설정
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.ReferencedDBC", IsVisible = true }); //DBC Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.ID", IsVisible = true }); //Message ID
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.MessageName", IsVisible = true }); //Message Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.Name", IsVisible = true }); //Signal Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "ValueType", IsVisible = true }); //Value type

			
		}

		private void Initialize_RegisterEvents()
        {
			GraphData.Graphs_CollectionChanged += OnGraphCollectionChanged;

			GraphData.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(GraphData.SelectedModel):
						SelectedItem = GraphData.SelectedModel;
						break;
				}
			};
		}


		private void OnGraphCollectionChanged(object sender, EventArgs e)
		{
			Graphs.Clear();
			bool isSelectedModelExist = false;
			foreach(var graph in GraphData.Graphs)
			{
				Graphs.Add(graph);
                if (!isSelectedModelExist 
					&& GraphData.SelectedModel != null 
					&& GraphData.SelectedModel.Tag == graph.Tag)
                {
					isSelectedModelExist = true;
                }
			}

			if (!isSelectedModelExist)
				GraphData.SelectedModel = Graphs.FirstOrDefault();
		}

		private void OnOpenColorPickerDialog(object param)
		{
			if (param is GraphRenderModel model)
			{
				Color? result = GraphCore.DialogService.ShowColorPickerDialog(model.BaseColor);
				if(result != null)
				{
					model.BaseColor = (Color)result;
					//paramColor = (Color)result;
				}
			}
		}

		public ICommand CMD_OpenColorPickerDialog { get; }

		public ObservableCollection<GraphModel> Graphs { get; } //절대 Collection의 crud를 직접하지말것. 무조건 GraphData.Graphs를 통해서만 수정되는 컬렉션 

		public GraphModel SelectedItem
        {
			get => GetProperty(() => SelectedItem);
            set
            {
				if(SetProperty(() => SelectedItem, value))
                {
					GraphData.SelectedModel = value;
                }
            }
        }

		public IList SelectedItems { get; set; } = new ObservableCollection<GraphModel>(); //직접 접근 금지. 순회할 때는 .Tolist() 등의 복사본 사용 필요
		public ObservableCollection<ColumnSetting> ColumnSettings { get; } = new ObservableCollection<ColumnSetting>(); //Column (name + visible) 을 알아보도록 만든 collection, visible 이 t, f냐에 따라 column이 보여짐
		public ObservableCollection<GridValueType> ValueTypes { get; } //value type combobox에 바인딩될 컬렉션, datasource는 모델로부터 받아옴.

		public ObservableCollection<GridPlotType> PlotTypes { get; }
	}
}
