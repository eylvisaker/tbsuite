using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class SymmetryList : List<Symmetry>
	{
		public void Add(Matrix m, IEnumerable<int> orbitals)
		{
			Symmetry s = new Symmetry(m);
			if (orbitals != null)
			{
				s.OrbitalTransform.AddRange(orbitals);
			}

			Add(s);
		}

		public  int TransformOrbital(List<int> orbitalMap, int orbitalIndex)
		{
			if (orbitalMap.Count > 0)
				return orbitalMap[orbitalIndex];
			else
				return orbitalIndex;
		}
	}
}
