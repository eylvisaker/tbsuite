using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
{
	class SymmetryList : List<Symmetry>
	{
		public void Add(Matrix m, IEnumerable<int> orbitals)
		{
			Symmetry s = new Symmetry(m);
			s.OrbitalTransform.AddRange(orbitals);
			Add(s);
		}
	}
}
