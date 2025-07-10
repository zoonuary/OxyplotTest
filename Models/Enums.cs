using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest
{
	//moduel behavior
	public enum eEVENT_BEHAVIOR_TYPE
	{
		START,
		STOP,
		ONEVENT,
		DBC_UPDATED
	}

	//plot mode
	public enum ePLOT_MODE 
	{ 
		LINE,
		BAR,
		POINT,
		AREA
	}

	//navigation page type
	public enum ePAGE_TYPE
	{
		SINGLE_Y,
		MULTIPLE_Y,
		SEPARATE_Y
	}

	//value Type
	public enum eVALUE_TYPE
	{
		RAW,
		PHYSICAL,
	}
	//class Enums
	//{
	//}
}
