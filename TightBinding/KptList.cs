
using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	public class KptList
	{
		List<KPoint> kpts = new List<KPoint>();
		protected int[] mesh;
		protected int[] shift;
		Dictionary<int, int> mNvalues = new Dictionary<int, int>();

		public KptList()
		{
		}

		public KptList Clone()
		{
			return CloneImpl();
		}

		protected void AddKpt(KPoint kpt)
		{
			int N = KptToInteger(kpt);

			if (mNvalues.ContainsKey(N) == false)
			{
				mNvalues.Add(N, kpts.Count);
			}

			kpts.Add(kpt);
		}
		protected bool ContainsKindex(int N)
		{
			return mNvalues.ContainsKey(N);
		}
		public int Kindex(int N)
		{
			return mNvalues[N];
		}

		protected virtual KptList CloneImpl()
		{
			KptList retval = (KptList)Activator.CreateInstance(this.GetType());

			CloneBaseImpl(retval);

			return retval;
		}
		protected void CloneBaseImpl(KptList retval)
		{
			retval.kpts.AddRange(kpts.Select(x => x.Clone()));

			foreach (var kvp in mNvalues)
			{
				retval.mNvalues.Add(kvp.Key, kvp.Value);
			}

			if (mesh != null)				retval.mesh = (int[])mesh.Clone();
			if (shift != null)				retval.shift = (int[])shift.Clone();
		}
		/// <summary>
		/// Override to implement Clone correctly.
		/// </summary>
		/// <param name="retval"></param>
		protected virtual void ProtectedClone(KptList retval)
		{
		}

		public static KptList DefaultPath()
		{
			const int pts = 40;
			KptList retval = new KptList();

			Vector3 Xpt = new Vector3(1, 0, 0) / 2;
			Vector3 Kpt = new Vector3(1, 1, 0) / 2;
			Vector3 Rpt = new Vector3(1, 1, 1) / 2;
			Vector3 Zpt = new Vector3(0, 0, 1) / 2;

			retval.Kpts.Add(new KPoint(Vector3.Zero));
			retval.AddPts(Vector3.Zero, Xpt, pts);
			retval.AddPts(Xpt, Kpt, pts);
			retval.AddPts(Kpt, Zpt, pts);
			retval.AddPts(Zpt, Rpt, pts);
			retval.AddPts(Rpt, Vector3.Zero, pts);

			return retval;
		}
		public static Vector3 CalcK(double dx, double dy, double dz)
		{
			return new Vector3(dx, dy, dz);
		}

		public int[] Mesh { get { return mesh; } }

		public int IndexOf(Vector3 kpt)
		{
			int newi, newj, newk;

			TranslateKpt(ref kpt);

			ReduceKpt(kpt, out newi, out newj, out newk);

			int N = KptToInteger(newi, newj, newk);

			return mNvalues[N];
		}

		private void TranslateKpt(ref Vector3 kpt)
		{
			for (int i = 0; i < 3; i++)
			{
				while (kpt[i] < -0.5) kpt[i] += 1;
				while (kpt[i] > 0.5) kpt[i] -= 1;
			}
		}

		/// <summary>
		/// Adds "count" points from the start to the end vectors.
		/// The start point is not added, and the end point is.
		/// </summary>
		/// <param name="start">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="end">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="count">
		/// A <see cref="System.Int32"/>
		/// </param>
		public void AddPts(Vector3 start, Vector3 end, int count)
		{
			if (count <= 0)
				throw new ArgumentException("Count must be positive.");

			Vector3 step = (end - start) / count;

			for (int i = 1; i < count; i++)
			{
				Kpts.Add(new KPoint(start + step * i));
			}

			Kpts.Add(new KPoint(end));
		}

		public List<KPoint> Kpts { get { return kpts; } }
		
		public static KptList GenerateMesh(int[] kgrid, int[] shift, bool includeEnds)
		{
			KptList retval = new KptList();

			if (shift == null)
				shift = new int[3];

			for (int i = 0; i < 3; i++)
				if (kgrid[i] == 1)
					shift[i] = 1;

			retval.mesh = (int[])kgrid.Clone();
			retval.shift = (int[])shift.Clone();

			int index = 0;
			for (int k = -kgrid[2] + shift[2]; k <= kgrid[2]; k += 2)
			{
				for (int j = -kgrid[1] + shift[1]; j <= kgrid[1]; j += 2)
				{
					for (int i = -kgrid[0] + shift[0]; i <= kgrid[0]; i += 2)
					{
						if (includeEnds == false)
						{
							if (k == kgrid[2]) break;
							if (j == kgrid[1]) break;
							if (i == kgrid[0]) break;
						}

						double dx = i * 0.5 / kgrid[0];
						double dy = j * 0.5 / kgrid[1];
						double dz = k * 0.5 / kgrid[2];

						Vector3 kptValue = new Vector3(dx, dy, dz);

						int N = retval.KptToInteger(i, j, k);
						int symN = N;

						System.Diagnostics.Debug.Assert(!(retval.ContainsKindex(N) && includeEnds == false));
						
						KPoint kpt = new KPoint(kptValue);
						kpt.Nvalue = N;

						if (includeEnds)
						{
							if (retval.ContainsKindex(N))
							{
								retval.kpts.Add(kpt);
								continue;
							}
						}

						retval.mNvalues.Add(N, retval.kpts.Count);
						retval.kpts.Add(kpt);
					}
				}
			}

			int count = kgrid[0] * kgrid[1] * kgrid[2];
			for (int i = 0; i < retval.kpts.Count; i++)
			{
				retval.kpts[i].Weight /= count;
			}

			return retval;
		}

		protected void ReduceKpt(Vector3 pt, out int newi, out int newj, out int newk)
		{
			newi = (int)Math.Round(mesh[0] * pt.X * 2);
			newj = (int)Math.Round(mesh[1] * pt.Y * 2);
			newk = (int)Math.Round(mesh[2] * pt.Z * 2);
		}

		protected int KptToInteger(int i, int j, int k)
		{
			i += mesh[0];
			j += mesh[1];
			k += mesh[2];

			while (i >= mesh[0] * 2) i -= mesh[0] * 2;
			while (j >= mesh[1] * 2) j -= mesh[1] * 2;
			while (k >= mesh[2] * 2) k -= mesh[2] * 2;

			return i + j * 1000 + k * 1000000;
		}
		protected int KptToInteger(KPoint qpt)
		{
			return KptToInteger( qpt.Value);
		}
		protected int KptToInteger(Vector3 qpt)
		{
			int i, j, k;
			ReduceKpt( qpt, out i, out j, out k);

			return KptToInteger(i, j, k);
		}

		public int IrreducibleIndex(KptList irredList, Vector3 qpt, out List<int> orbitalMap)
		{
			int newi, newj, newk;
			TranslateKpt(ref qpt);
			ReduceKpt(qpt, out newi, out newj, out newk);

			int N = KptToInteger(newi, newj, newk);

			KPoint irrdKpt = Kpts[mNvalues[N]];

			if (irrdKpt.ReducingSymmetry != null)
				orbitalMap = irrdKpt.ReducingSymmetry.OrbitalTransform;
			else
				orbitalMap = new List<int>();

			return irredList.mNvalues[N];
		}

		public int IrreducibleIndex(Vector3 kpt, Lattice lattice, SymmetryList symmetries, out List<int> orbitalMap)
		{
			for (int s = 0; s < symmetries.Count; s++)
			{
				int newi, newj, newk;
				Vector3 newKpt = symmetries[s].Inverse * kpt;

				ReduceKpt(newKpt, out newi, out newj, out newk);

				int N = KptToInteger(newi, newj, newk);

				if (ContainsKindex(N))
				{
					int index = Kindex(N);

					orbitalMap = symmetries[s].OrbitalTransform;
					return index;
				}
			}

			throw new Exception(string.Format("Could not find k-point {0}", kpt));
		}

		public override string ToString()
		{
			return string.Format("K-points: {0}", kpts.Count);
		}

		public void SetTemperature(double temperature, double mu)
		{
			double beta = 1 / temperature;

			foreach (var k in kpts)
			{
				foreach (Wavefunction wfk in k.Wavefunctions)
				{
					wfk.FermiFunction = FermiFunction(wfk.Energy - mu, beta);
				}
			}
		}


		private double FermiFunction(double energy, double beta)
		{
			return 1.0 / (Math.Exp(beta * energy) + 1);
		}

		public virtual KptList CreateIrreducibleMesh(SymmetryList symmetryList)
		{
			KptList retval = new KptList();
			FillIrreducibleMesh(symmetryList, retval);

			return retval;
		}

		protected void FillIrreducibleMesh(SymmetryList symmetryList, KptList retval)
		{
			retval.mesh = (int[]) mesh.Clone();
			retval.shift = (int[])shift.Clone();

			foreach (KPoint kpt in this.Kpts)
			{
				bool found = false;
				int N = KptToInteger(kpt);
				int symN = 0;
				Symmetry s = null;

				foreach (Symmetry sym in symmetryList)
				{
					Vector3 trans_k = sym.Inverse * kpt;

					symN = KptToInteger(trans_k);

					if (retval.mNvalues.ContainsKey(symN))
					{
						s = sym;
						found = true;
						break;
					}
				}

				if (found)
				{
					kpt.ReducingSymmetry = s;
					retval.mNvalues[N] = retval.mNvalues[symN];
					retval.kpts[retval.mNvalues[N]].Weight += kpt.Weight;
				}
				else
				{
					retval.mNvalues[N] = retval.kpts.Count;
					retval.kpts.Add(kpt.Clone());
				}
			}
		}

		public void FillWavefunctions(KptList kptList, SymmetryList symmetries)
		{
			foreach (KPoint kpt in kptList.Kpts)
			{
				bool found = false;
				int N = KptToInteger(kpt);
				int symN = 0;
				
				foreach (Symmetry sym in symmetries)
				{
					Vector3 trans_k = sym.Inverse * kpt;

					symN = KptToInteger(trans_k);

					int index = mNvalues[symN];

					if (Kpts[index].Nvalue != symN)
						continue;

					found = true;

					kpt.SetWavefunctions(Kpts[index], symmetries, sym.OrbitalTransform);
					break;
				}

				if (!found)
					throw new Exception();
			}
		}
	}
}