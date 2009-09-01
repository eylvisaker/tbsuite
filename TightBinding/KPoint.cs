using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBinding
{
	public class KPoint
	{
		public Vector3 Value;
		public double Weight = 1;
		public string Name;
		List<List<int>> mOrbitalTransform = new List<List<int>>();
		List<Wavefunction> wfk = new List<Wavefunction>();

		public KPoint(Vector3 v)
		{
			Value = v;	
		}
		public static implicit  operator Vector3(KPoint p)
		{
			return p.Value;	
		}

		public IEnumerable<int> GetEquivalentOrbitals(int orbital)
		{
			foreach (var xform in mOrbitalTransform)
			{
				foreach (int orb in xform)
				{
					if (orb != orbital)
						yield return orb;
				}
			}
		}

		public void AddOrbitalSymmetry(List<int> orbitals)
		{
			bool significant = false;

			if (orbitals == null || orbitals.Count == 0)
				return;

			for (int i = 0; i < orbitals.Count; i++)
			{
				if (i != orbitals[i])
					significant = true;
			}

			if (!significant)
				return;

			mOrbitalTransform.Add(orbitals);
		}

		public List<Wavefunction> Wavefunctions { get { return wfk; } }
		public IEnumerable<List<int>> OrbitalTransform { get { return mOrbitalTransform; } }
	}
}
