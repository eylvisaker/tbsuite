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
		List<int> OrbitalTransform = new List<int>();
		List<Wavefunction> wfk = new List<Wavefunction>();

		public KPoint(Vector3 v)
		{
			Value = v;	
		}
		public static implicit  operator Vector3(KPoint p)
		{
			return p.Value;	
		}

		public int OrbitalMap(int orbital)
		{
			if (OrbitalTransform.Count == 0)
				return orbital;

			return OrbitalTransform[orbital];
		}

		bool HaveOrbitalTransform
		{
			get { return OrbitalTransform.Count != 0; }
		}
		internal void SetOrbitalSymmetry(List<int> orbitals)
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

			if (HaveOrbitalTransform == false)
			{
				OrbitalTransform.AddRange(orbitals);
			}
			else
			{
				if (orbitals.Count != OrbitalTransform.Count)
				{
					throw new Exception("Incomplete symmetry information detected!");
				}
				for (int i = 0; i < OrbitalTransform.Count; i++)
				{
					if (OrbitalTransform[i] != orbitals[i])
						throw new Exception("Incompatible symmetry detected!");
				}
			}

		}

		public List<Wavefunction> Wavefunctions { get { return wfk; } }
	}
}
