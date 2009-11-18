
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

		public int IndexOf(Vector3 kpt, int start)
		{
			for (int i = start; i < kpts.Count; i++)
			{
				Vector3 diff = kpts[i].Value - kpt;

				if (diff.X > 0.01) continue;
				if (diff.Y > 0.01) continue;
				if (diff.Z > 0.01) continue;

				if (diff.MagnitudeSquared < 1e-6)
					return i;
			}

			return -1;
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

						retval.kpts.Add(kpt);

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

		[Obsolete]
		public int AllKindex(Vector3 kpt)
		{
			int newi, newj, newk;
				
			ReduceKpt(kpt, out newi, out newj, out newk);
			
			int N = KptToInteger(newi, newj, newk);

			return mNvalues[N];
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

		internal KptList CreateIrreducibleMesh(SymmetryList symmetryList)
		{
			return this;
		}
	}
}