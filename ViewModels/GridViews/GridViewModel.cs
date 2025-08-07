using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
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
		public GraphCore GraphCore { get; }
		public GraphData GraphData { get; }
		public GridControl GridControl { get; set; }
		public GridPopupHelper GridPopupHelper { get; }
		public GridViewModel(GraphCore graphCore)
		{
			//데이터 초기 설정
			GraphCore = graphCore;
			GraphData = GraphCore.GraphData;
			GridPopupHelper = new GridPopupHelper(GraphCore);


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

			PlotTypes = new ObservableCollection<GridPlotType>
			{
				new GridPlotType("Line", ePLOT_MODE.LINE),
				new GridPlotType("Point", ePLOT_MODE.POINT),
				new GridPlotType("Line + Point", ePLOT_MODE.LINE_POINT)
			};
		}

		#region Properties

		public ICommand CMD_OpenColorPickerDialog { get; }

		public ObservableCollection<GraphModel> Graphs { get; } //절대 Collection의 crud를 직접하지말것. 무조건 GraphData.Graphs를 통해서만 수정되는 컬렉션 

		public GraphModel SelectedItem
		{
			get => GetProperty(() => SelectedItem);
			set
			{
				if (SetProperty(() => SelectedItem, value))
				{
					GraphData.SelectedModel = value;
				}
			}
		}

		public string ColumnChooserButtonText { get; set; } = "Show Column Chooser...";

		public bool? IsAllColumnsVisible
		{
			get
			{
				if (ColumnSettings.All(x => x.IsVisible == true)) return true;
				if (ColumnSettings.All(x => x.IsVisible == false)) return false;
				return null;
			}
			set
			{
				if (value != null)
				{
					foreach (var column in ColumnSettings)
					{
						column.IsVisible = value;
					}
				}
				SetProperty(() => IsAllColumnsVisible, value);
			}
		}

		public IList SelectedItems { get; set; } = new ObservableCollection<GraphModel>(); //직접 접근 금지. 순회할 때는 .Tolist() 등의 복사본 사용 필요
		public ObservableCollection<ColumnSetting> ColumnSettings
		{
			get => GetProperty(() => ColumnSettings);
			set => SetProperty(() => ColumnSettings, value);
		}

		public ObservableCollection<GridValueType> ValueTypes { get; } //value type combobox에 바인딩될 컬렉션, datasource는 모델로부터 받아옴.
		public ObservableCollection<GridPlotType> PlotTypes { get; }

        #endregion

        #region Methods
        private void Initialize_GridveiwSetting()
        {
			ColumnSettings = new ObservableCollection<ColumnSetting>();
			//컬럼 초기 설정 인덱스 매칭으로 진행하는게 최선으로 보여짐(IsVisible property를 xaml에서 직접 접근 불가능함)
			ColumnSettings.Add(new ColumnSetting { Header = "Dbc", IsVisible = true }); 
			ColumnSettings.Add(new ColumnSetting { Header = "Message ID", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Message Name", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Signal Name", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Value Type", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Plot Type", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Color", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Y", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "Y time", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "dY", IsVisible = true });
			ColumnSettings.Add(new ColumnSetting { Header = "dY time", IsVisible = true });

			foreach (var column in ColumnSettings)
            {
				column.PropertyChanged += (s, e) =>
				{
					if (e.PropertyName == nameof(column.IsVisible))
					{
						RaisePropertiesChanged(nameof(IsAllColumnsVisible));
						RaisePropertiesChanged(nameof(ColumnSettings));
					}
				};
            }
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
        #endregion
	}
}
