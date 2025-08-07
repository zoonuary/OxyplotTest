using HPT.DBCParser.Models;
using OxyTest.Models.Graph;
using OxyTest.ViewModels.Dialogs;
using OxyTest.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;




namespace OxyTest.Services
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left, Top, Right, Bottom;
	}

	public class DialogService
	{
		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);
		private Visual MainVisual { get; }

		public DialogService(Visual visual)
		{
			MainVisual = visual;
		}

		public Color? ShowColorPickerDialog(Color currentColor)
		{
			var dialog = new ColorPickerDialog(currentColor);
			if(ShowDialogCenterdOn(dialog) == true)
			{
				return dialog.SelectedColor;
			}
			return null;
		}

		/// <summary>
		/// SignalAddDialog를 생성하는 함수
		/// Dialog가 닫히지 않아도 데이터가 추가될 수 있음으로 callbackfunction 을 parameter로 추가하여 
		/// 데이터가 추가될 때마다 Function callbackAction 실행
		/// </summary>
		/// <param name="callbackAction"> GraphModel을 parameter로 선택 완료시 실행될 함수 </param>
		/// <param name="visual"> dialog를 생성하는 usercontrol 혹은 window, dialog 위치를 잡아주는 hwnd 계산용 </param>
		public void ShowSignalAddDialog(Action<GraphModel> callbackAction)
		{
			var dialog = new SignalAddDialog((NodeItem item) =>
			{
				//더 큰 작업이 필요한 경우, model builder 사용 필요

				//signal 관련 정보데이터 파싱
				var signalData = new SignalDataModel(
					referencedDBC: item.ReferencedDBC,
					name: item.Name,
					id: item.Signal.ID,
					messageName: item.MessageName,
					factor: item.Signal.Factor,
					offset: item.Signal.Offset,
					length: item.Signal.Length,
					startbit: item.Signal.StartBit,
					isUnsigned: item.Signal.ValueType == DbcValueType.Unsigned ? true : false,
					valueDescriptions: item.ValueDescriptions
					);

				//파싱 완료된 모델을 담아 함수 실행
				callbackAction?.Invoke(new GraphModel(signalData));
			});
			ShowDialogCenterdOn(dialog);
		}

		//화면 중앙에 dialog를 띄우도록 유도하는 함수
		private bool? ShowDialogCenterdOn(Window dialog)
		{
			IntPtr hwnd = GetWindowHandle();
			RECT rect = new RECT();
			if(GetWindowRect(hwnd, ref rect))
			{
				//dialog 위치 수동 계산
				dialog.WindowStartupLocation = WindowStartupLocation.Manual;
				dialog.SourceInitialized += (_, __) =>
				{
					double width = dialog.ActualWidth > 0 ? dialog.ActualWidth : dialog.Width;
					double height = dialog.ActualHeight > 0 ? dialog.ActualHeight : dialog.Height;

					dialog.Left = rect.Left + ((rect.Right - rect.Left) - width) / 2;
					dialog.Top = rect.Top + ((rect.Bottom - rect.Top) - height) / 2;
				};

				new WindowInteropHelper(dialog).Owner = hwnd;
			}
			return dialog.ShowDialog();
		}

		//부모 window(elementhost)의 handle을 가져옴
		private IntPtr GetWindowHandle()
		{
			var hwndSource = PresentationSource.FromVisual(MainVisual) as HwndSource;
			if (hwndSource != null)
			{
				//그냥 가져오면 언제나 document위주로 중심을 잡기 때문에, main window에서 어디에 도킹되느냐에따라 popup 위치가 달라짐.
				//원하는 동작은 main window의 정중앙에서 popup되는걸 바라기때문에 elementHost까지 찾아 handle을 리턴.
				var elementHost = System.Windows.Forms.Control.FromChildHandle(hwndSource.Handle);
				return elementHost?.FindForm()?.Handle ?? IntPtr.Zero;
			}
			return IntPtr.Zero;
		}
	}
}