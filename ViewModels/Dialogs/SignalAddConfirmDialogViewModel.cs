using DevExpress.Mvvm;
using OxyTest.Views.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OxyTest.ViewModels.Dialogs
{
	public class SignalAddConfirmDialogViewModel : ViewModelBase
	{
		public SignalAddConfirmDialogViewModel(List<string> signalNames, SignalAddConfirmDialog dialog)
		{
			PreviewItems = new ObservableCollection<string>(signalNames);

			CMD_ToggleExpanded = new DelegateCommand(() => IsExpanded = !IsExpanded);
			CMD_Confirm = new DelegateCommand(() => dialog.DialogResult = true);
			CMD_Cancel = new DelegateCommand(() => dialog.DialogResult = false);
		}

		public ObservableCollection<string> PreviewItems { get; set; } = new ObservableCollection<string>();

		public ICommand CMD_ToggleExpanded { get; }
		public ICommand CMD_Confirm { get; }
		public ICommand CMD_Cancel { get; }

		public bool IsExpanded
		{
			get => GetProperty(() => IsExpanded);
			set
			{
				SetProperty(() => IsExpanded, value);
				RaisePropertiesChanged(nameof(ToggleButtonText));
			}
		}

		public string ToggleButtonText => IsExpanded ? "접기 ▲" : "펼쳐보기 ▼";

		public string SummaryText => $"총 {PreviewItems.Count}개 항목";
	}
}
