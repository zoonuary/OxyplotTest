using DevExpress.Mvvm;
using OxyTest.Composition;
using OxyTest.Data;
using OxyTest.Models.Graph;
using OxyTest.ViewModels.GridVIews.Internals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace OxyTest.ViewModels
{
	public class GridViewModel : ViewModelBase
	{
		private GraphCore GraphCore { get; }
		public GraphData GraphData { get; }
		public GridViewModel(GraphCore graphCore)
		{
			//데이터 초기 설정
			GraphCore = graphCore;
			GraphData = GraphCore.GraphData;

			//Graphs => view 에 바인딩되는 모델 컬렉션. 
			Graphs = new ObservableCollection<GraphModel>(GraphData.Graphs);
			GraphData.Graphs_CollectionChanged += OnGraphCollectionChanged;

			//commands
			CMD_OpenColorPickerDialog = new DelegateCommand<object>(OnOpenColorPickerDialog);
			//컬럼 초기 설정
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.ReferencedDBC", IsVisible = true }); //DBC Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.ID", IsVisible = true }); //Message ID
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.MessageName", IsVisible = true }); //Message Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "SignalDataModel.Name", IsVisible = true }); //Signal Name
			ColumnSettings.Add(new ColumnSetting { FieldName = "ValueType", IsVisible = true }); //Value type


			//value type 선택 combobox용 collection
			ValueTypes = new ObservableCollection<GridValueType>
			{
				new GridValueType("Raw", eVALUE_TYPE.RAW),
				new GridValueType("Physical", eVALUE_TYPE.PHYSICAL)
			};
		}

		private void OnGraphCollectionChanged(object sender, EventArgs e)
		{
			Graphs.Clear();
			foreach(var graph in GraphData.Graphs)
			{
				Graphs.Add(graph);
			}
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
		public ObservableCollection<ColumnSetting> ColumnSettings { get; } = new ObservableCollection<ColumnSetting>(); //Column (name + visible) 을 알아보도록 만든 collection, visible 이 t, f냐에 따라 column이 보여짐
		public ObservableCollection<GridValueType> ValueTypes { get; } //value type combobox에 바인딩될 컬렉션, datasource는 모델로부터 받아옴.
	}
}
