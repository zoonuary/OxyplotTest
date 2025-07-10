using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.ViewModels.GridVIews.Internals
{
	public class GridValueType
	{
		public string DisplayName { get; set; }
		public eVALUE_TYPE ValueType { get; set; }
		public GridValueType(string displayname, eVALUE_TYPE valuetype)
		{
			DisplayName = displayname;
			ValueType = valuetype;
		}
	}
}
