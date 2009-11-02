using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace FploWannierConverter
{
	class Grid
	{
		public Grid()
		{
			Delta = new Vector3[3];
			GridSize = new int[3];
		}

		public int[] GridSize { get; private set; }
		public Vector3 Origin { get; set; }
		public Vector3[] Delta { get; private set; }
	}
}
