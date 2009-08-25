using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
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
	}
}
