using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using HPT.DBCParser;
using HPT.DBCParser.Models;
using HPT.DBCParser.Parsers;
using OxyTest.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OxyTest.ViewModels.Dialogs
{
	public class SignalAddDialogViewModel : ViewModelBase
	{
		public SignalAddDialogViewModel(Action<NodeItem> addToGraph, SignalAddDialog view)
		{
			AddToGraph = addToGraph;
			View = view;

			Loaded = new DelegateCommand(OnLoaded);
			Closed = new DelegateCommand(OnClosed);
			CMD_Append = new DelegateCommand<object>(OnAppend);
			CMD_AddToGraph = new DelegateCommand<NodeItem>(OnAddToGraph);
			CMD_OK = new DelegateCommand(OnOK);
			CMD_TreeListDoubleClick = new DelegateCommand<object>(OnTreeListDoubleClick);
			TreeNodeCollections = new ObservableCollection<NodeItem>();
		}
		#region Properties
		private Window View { get; set; }

		private readonly Action<NodeItem> AddToGraph;

		public ICommand CMD_AddToGraph { get; }
		public ICommand Loaded { get; }
		public ICommand Closed { get; }
		public ICommand CMD_Append { get; }
		public ICommand CMD_OK { get; }
		public ICommand CMD_TreeListDoubleClick { get; }
		public ICommand CMD_TreeSelectionChanged { get; }

		public ObservableCollection<NodeItem> TreeNodeCollections
		{
			get => GetProperty(() => TreeNodeCollections);
			set => SetProperty(() => TreeNodeCollections, value);
		}

		public NodeItem SelectedItem
		{
			get => GetProperty(() => SelectedItem);
			set => SetProperty(() => SelectedItem, value);
		}

		public ObservableCollection<NodeItem> SelectedItems
		{
			get => GetProperty(() => SelectedItems);
			set => SetProperty(() => SelectedItems, value);
		}
		#endregion

		#region Methods
		private void OnAddToGraph(NodeItem signal)
		{
			AddToGraph?.Invoke(signal);
		}

		private void OnAppend(object param)
		{
			if (param is TreeListControl treeControl)
			{
				var selectedNodes = treeControl.View.GetSelectedRows()
									.Select(node => treeControl.GetRow(node.RowHandle)).OfType<NodeItem>().ToList();

				List<string> strList = new List<string>();
				List<NodeItem> signalNodes = new List<NodeItem>();

				foreach (NodeItem node in selectedNodes)
				{
					if (node.Signal != null)
					{
						strList.Add(node.Signal.Name);
						signalNodes.Add(node);
					}
				}

				if (ShowConfirmDialog(strList))
				{
					foreach (NodeItem node in signalNodes)
					{
						OnAddToGraph(node);
					}
				}
			}
		}

		private void OnOK() //Dlg send OK
		{
			View.DialogResult = true;
		}

		private void OnTreeListDoubleClick(object param)
		{
			if (param is NodeItem item)
			{
				if (item.Signal != null) //signal 을 가진다 -> signal model이다. => 혹시 불가능한 조건이라면, 'Children' 이용
				{
					if (ShowConfirmDialog(new List<string> { item.Signal.Name }))
					{
						OnAddToGraph(item);
					}
				}
			}
		}

		private bool ShowConfirmDialog(List<string> signalName)
		{
			var confirmDialog = new SignalAddConfirmDialog(signalName) { Owner = View, WindowStartupLocation = WindowStartupLocation.CenterOwner };

			if (confirmDialog.ShowDialog() == true)
			{
				return true;
			}
			return false;
		}

		private void OnLoaded()
		{
			Console.WriteLine(typeof(DevExpress.Xpf.Grid.TreeListControl).Assembly.FullName);

			if (TreeNodeCollections.Count > 0)
			{
				TreeNodeCollections.Clear();
			}

			foreach (var dbc in DBCController.Instance.GetAllDatabase().ToList())
			{
				SetTree(dbc);
			}
		}

		private void OnClosed()
		{
			TreeNodeCollections.Clear();
		}

		#endregion

		#region SubMethods
		private void SetTree(Dbc dbc)
		{
			NodeItem nodesNode = new NodeItem("Node");
			NodeItem messagesNode = new NodeItem("Message");
			NodeItem signalsNode = new NodeItem("Signals");
			NodeItem dbcNode = new NodeItem(dbc.DBCName)
			{
				Children = { nodesNode, messagesNode, signalsNode }
			};
			TreeNodeCollections.Add(dbcNode);

			foreach (var node in dbc.Nodes)
			{
				NodeItem txMessage = new NodeItem("TxMessage");
				NodeItem rxMessage = new NodeItem("RxMessage");
				NodeItem singleNode = new NodeItem(node.Name)
				{
					Children = { txMessage, rxMessage }
				};
				nodesNode.Children.Add(singleNode);
			}


			foreach (var message in dbc.Messages)                                                   //1. 메시지 추가
			{
				string messageID = "0x" + Convert.ToString(message.ID, 16).ToUpper();
				NodeItem msgNode = new NodeItem(message.Name, messageID, message.Name, message.Transmitter, message.Comment);
				messagesNode.Children.Add(msgNode);
				//2. 메시지에 signal 추가
				foreach (var signal in message.Signals)
				{
					NodeItem signalNode = new NodeItem(signal.Name, messageID, message.Name, message.Transmitter, signal.Comment, signal, dbc.DBCName, signal.ValueTableMap);
					msgNode.Children.Add(signalNode);
					signalsNode.Children.Add(signalNode);                                           //3. signalsNode에 나온 모든 signal 추가
				}

				foreach (var node in dbc.Nodes)                                                      //4. Node를 탐색해 TX, Rx message 들이 있으면 추가
				{

					if (message.Transmitter.Equals(node.Name))
					{
						NodeItem txmessage = nodesNode.Children.FirstOrDefault(x => x.Name.Equals(node.Name))?.Children.FirstOrDefault(x => x.Name.Equals("TxMessage"));
						txmessage?.Children.Add(msgNode);
					}

					if (message.Signals.Any(signal => signal.Receiver.Contains(node.Name)))
					{
						NodeItem rxmessage = nodesNode.Children.FirstOrDefault(x => x.Name.Equals(node.Name))?.Children.FirstOrDefault(x => x.Name.Equals("RxMessage"));
						rxmessage?.Children.Add(msgNode);
					}
				}
			}
		}
		#endregion
	}

	public class NodeItem
	{
		public string Name { get; set; }
		public string ID { get; set; }
		public string MessageName { get; set; }
		public string Transmitter { get; set; }
		public string Comment { get; set; }
		public Signal Signal { get; set; }
		public string ReferencedDBC { get; set; }
		public ObservableCollection<NodeItem> Children { get; set; }
		public IReadOnlyDictionary<int, string> ValueDescriptions { get; }

		public NodeItem(string name)
		{
			Name = name;
			Children = new ObservableCollection<NodeItem>();
		}

		public NodeItem(string name, string id, string transmitter)
		{
			Name = name;
			ID = id;
			Transmitter = transmitter;
			Children = new ObservableCollection<NodeItem>();
		}

		public NodeItem(string name, string id, string messageName, string transmitter, string comment, Signal signal = null, string referencedDBC = "", IReadOnlyDictionary<int, string> valueDescriptions = null)
		{
			Name = name;
			ID = id;
			MessageName = messageName;
			Transmitter = transmitter;
			Comment = comment;
			Signal = signal;
			ReferencedDBC = referencedDBC;
			ValueDescriptions = valueDescriptions;
			Children = new ObservableCollection<NodeItem>();
		}

		public NodeItem Clone()
		{
			return new NodeItem(this.Name, this.ID, this.MessageName, this.Transmitter, this.Comment)
			{
				Children = this.Children
			};
		}
	}
}
