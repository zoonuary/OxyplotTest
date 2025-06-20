using HPT.DBCParser.Models;
using OxyTest.Models.Graph;
using OxyTest.ViewModels.Dialogs;
using OxyTest.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Services
{
	public class DialogService
	{
		//parent form handle
		private IntPtr Hwnd { get; set; } = IntPtr.Zero;
		public DialogService()
		{

		}

		public void Init(IntPtr hwnd)
		{
			Hwnd = hwnd;
		}

		/// <summary>
		/// SignalAddDialog를 생성하는 함수
		/// Dialog가 닫히지 않아도 데이터가 추가될 수 있음으로 callbackfunction 을 parameter로 추가하여 
		/// 데이터가 추가될 때마다 Function callbackAction 실행
		/// </summary>
		/// <param name="callbackAction"> GraphModel을 parameter로 선택 완료시 실행될 함수 </param>
		public void ShowSignalAddDialog(Action<GraphModel> callbackAction)
		{
			var dialog = new SignalAddDialog((NodeItem item) =>
			{
				//더 큰 작업이 필요한 경우, model builder 사용 필요

				//signal 관련 정보데이터 파싱
				var signalData = new SignalDataModel(
					referencedDBC: item.ReferencedDBC,
					name : item.Name,
					id: item.Signal.ID,
					messageName: item.MessageName,
					factor: item.Signal.Factor,
					offset: item.Signal.Offset,
					length: item.Signal.Length,
					startbit: item.Signal.StartBit,
					isUnsigned: item.Signal.ValueType == DbcValueType.Unsigned ? true : false
					);

				//파싱 완료된 모델을 담아 함수 실행
				callbackAction?.Invoke(new GraphModel(signalData));
			});

			dialog.ShowDialog();
		}


	}
}