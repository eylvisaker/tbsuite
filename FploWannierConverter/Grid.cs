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

		Vector3[] spanVecs;

		public Vector3[] SpanVectors
		{
			get
			{
				if (spanVecs == null || spanVecs[0].Magnitude == 0)
				{
					spanVecs = new Vector3[3];

					for (int i = 0; i < 3; i++)
					{
						spanVecs[i] = Delta[i] * (GridSize[i] - 1);
					}
				}

				return spanVecs;
			}
		}
	}
}
