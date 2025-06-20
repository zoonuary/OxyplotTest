using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Models.Graph
{
	public class SignalDataModel : INotifyPropertyChanged
	{
		//GraphModel의 signal property들을 분리하기위해 만든 클래스
		public SignalDataModel(string referencedDBC, string name, uint id, string messageName, double factor, double offset, ushort length, ushort startbit, bool isUnsigned)
		{
			ReferencedDBC = referencedDBC;
			Name = name;
			ID = id;
			MessageName = messageName;
			Factor = factor;
			Offset = offset;
			Length = length;
			StartBit = startbit;
			IsUnsigned = isUnsigned;
		}

		public string ReferencedDBC { get; }
		public string Name { get; }
		public uint ID { get; }
		public string MessageName { get; }
		public double Factor { get; }
		public double Offset { get; }
		public ushort Length { get; }
		public ushort StartBit { get; }
		public bool IsUnsigned { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
