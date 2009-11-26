using System;
using System.Collections.Generic;
using System.Linq;
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

		public bool Contains(Symmetry s)
		{
			for (int i = 0; i < Count; i++)
			{
				Symmetry t = this[i];

				if ((t.Value - s.Value).IsZero == false)
					continue;
				if ((t.Translation - s.Translation).Magnitude > 1e-12)
					continue;

				return true;
			}

			return false;
		}

		public int TransformOrbital(List<int> orbitalMap, int orbitalIndex)
		{
			if (orbitalMap.Count > 0)
				return orbitalMap[orbitalIndex];
			else
				return orbitalIndex;
		}

		internal SymmetryList Clone()
		{
			SymmetryList p = new SymmetryList();
			p.AddRange(this.Select(x => x.Clone()));
			return p;
		}
	}
}
