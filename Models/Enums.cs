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
		CLEAR,
		ONEVENT,
		DBC_UPDATED
	}

	public enum eLOCAL_STATUS
    {
		LIVEUPDATE,
		PAUSED,
		STOPPED
    }

	//plot mode
	public enum ePLOT_MODE 
	{ 
		LINE,
		BAR,
		POINT,
		AREA,
		LINE_POINT
	}

	//navigation page type
	public enum ePAGE_TYPE
	{
		SINGLE_Y,
		MULTIPLE_Y,
		SEPARATE_Y
	}

	//plotview cursor type
	public enum eCURSOR_TYPE
    {
		DEFAULT,
		MEASUERMENT,
		DIFFERENCE
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
