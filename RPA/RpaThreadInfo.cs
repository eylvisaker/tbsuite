using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TightBindingSuite
{
	class RpaThreadInfo
	{
		public RpaThreadInfo()
		{
			RpaParams = new List<RpaParams>();
		}

		public TightBinding tb { get; set; }
		public List<RpaParams> RpaParams { get; private set; }
		public KptList qpts { get; set; }
		public bool PrimaryThread { get; set; }
		public Thread Thread { get; set; }

	}
}
