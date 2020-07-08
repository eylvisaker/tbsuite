using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TightBindingSuite
{
	public class OrbitalMap
	{
		int[] orb;

		public OrbitalMap(int orbitals)
		{
			orb = new int[orbitals];
		}

		public int this[int orbitalIndex]
		{
			get { return orb[orbitalIndex]; }
			set { orb[orbitalIndex] = value; }
		}

	}
}
