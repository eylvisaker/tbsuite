using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	public class KPoint
	{
		public Vector3 Value;
		public double Weight = 1;
		public string Name;
		List<List<int>> mOrbitalTransform = new List<List<int>>();
		List<Wavefunction> wfk = new List<Wavefunction>();
		public int Nvalue { get; set; }

		public KPoint(Vector3 v)
		{
			Value = v;	
		}

		public KPoint Clone()
		{
			KPoint retval = new KPoint(Value);

			retval.Weight = Weight;
			retval.Name = Name;

			foreach (var transform in mOrbitalTransform)
			{
				List<int> newxform = new List<int>();
				newxform.AddRange(transform);
				retval.mOrbitalTransform.Add(newxform);
			}

			retval.wfk.AddRange(wfk.Select(x => x.Clone()));

			return retval;
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

		public override string ToString()
		{
			return base.ToString() + Value.ToString();
		}

		internal void SetWavefunctions(Matrix eigenvals, Matrix eigenvecs)
		{
			for (int n = 0; n < eigenvals.Rows; n++)
			{
				var wfk = new Wavefunction(eigenvecs.Rows);

				wfk.Energy = eigenvals[n, 0].RealPart;

				for (int c = 0; c < eigenvecs.Rows; c++)
				{
					wfk.Coeffs[c] = eigenvecs[c, n];
				}

				this.wfk.Add(wfk);
			}
		}

		internal void SetWavefunctions(KPoint otherPt, SymmetryList symmetries, List<int> orbitalMap)
		{
			if (this.wfk.Count != 0) throw new InvalidOperationException();

			for (int n = 0; n < otherPt.Wavefunctions.Count; n++)
			{
				var otherWfk = otherPt.Wavefunctions[n];
				var wfk = new Wavefunction(otherWfk.Coeffs.Length);

				wfk.Energy = otherWfk.Energy;
				wfk.FermiFunction = otherWfk.FermiFunction;

				for (int k = 0; k < otherWfk.Coeffs.Length; k++)
				{
					int newOrb = symmetries.TransformOrbital(orbitalMap, k);

					System.Diagnostics.Debug.Assert(wfk.Coeffs[newOrb] == 0);
					wfk.Coeffs[newOrb] = otherWfk.Coeffs[k];

				}

				this.wfk.Add(wfk);
			}
		}


	}
}
