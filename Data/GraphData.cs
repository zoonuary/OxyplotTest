using OxyTest.Models.Graph;
using OxyTest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		}




		private readonly List<GraphModel> graphs = new List<GraphModel>();
		public List<GraphModel> Graphs
		{
			get
			{
				lock (_lock)
					return graphs.ToList();
			}
		}

		public void AddGraph(GraphModel model)
		{
			lock (_lock)
				graphs.Add(model);
		}

		public bool RemoveGraph(GraphModel model)
		{
			lock (_lock)
				return graphs.Remove(model);
		}

		public void ClearGraphs()
		{
			lock (_lock)
				graphs.Clear();
		}


	}
}
