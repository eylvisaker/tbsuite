
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBinding
{
	public class KptList
	{
		List<KPoint> allKpts = new List<KPoint>();
		List<KPoint> kpts = new List<KPoint>();
		List<Tetrahedron> tets = new List<Tetrahedron>();
		int[] mesh;
		int[] shift;
		Dictionary<int, int> Nvalues = new Dictionary<int, int>();

		Vector3 sdir, tdir, origin;

		public KptList()
		{
		}

		public static KptList DefaultPath(Lattice l)
		{
			const int pts = 40;
			KptList retval = new KptList();

			retval.Kpts.Add(new KPoint(Vector3.Zero));
			retval.AddPts(Vector3.Zero, l.G1 * Math.PI, pts);
			retval.AddPts(l.G1 * Math.PI, (l.G1 + l.G2) * Math.PI, pts);
			retval.AddPts((l.G1 + l.G2) * Math.PI, l.G2 * Math.PI, pts);
			retval.AddPts(l.G2 * Math.PI, l.G3 * Math.PI, pts);
			retval.AddPts(l.G3 * Math.PI, Vector3.Zero, pts);

			return retval;
		}
		public static Vector3 CalcK(Lattice l, double dx, double dy, double dz)
		{
			return dx * l.G1 + dy * l.G2 + dz * l.G3;
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
		public List<KPoint> AllKpts { get { return allKpts; } }

		public List<Tetrahedron> Tetrahedrons { get { return tets; } }

		public static KptList GeneratePlane(Lattice lattice, Vector3[] points, SymmetryList syms, int[] qgrid)
		{
			KptList qmesh = GenerateMesh(lattice, qgrid, null, syms, true, true);

			Vector3 diff_1 = points[1] - points[0];
			Vector3 diff_2 = points[2] - points[0];
			Vector3 norm = Vector3.CrossProduct(diff_1, diff_2);

			KptList retval = new KptList();

			retval.mesh = (int[])qgrid.Clone();
			retval.shift = new int[3];

			int zmax = qgrid[2] * 2;
			int ymax = qgrid[1] * 2;
			int xmax = qgrid[0] * 2;

			int index = 0;
			for (int i = 0; i < qmesh.AllKpts.Count; i++)
			{
				var qpt = qmesh.AllKpts[i];

				Vector3 diff = qpt.Value - points[0];
				double dot = Math.Abs(diff.DotProduct(norm));

				if (dot < 1e-8)
				{
					retval.allKpts.Add(qpt);

					int N = retval.CalcN(lattice, qpt);
					int symN = N;
					bool foundSym = false;
					List<int> orbitals = null;

					foreach (var symmetry in syms)
					{
						Vector3 pt = qpt.Value;
						Vector3 Tpt = symmetry.Value * pt;

						Vector3 red = lattice.ReducedCoords(Tpt, true);

						int newi = (int)Math.Round(xmax * red.X );
						int newj = (int)Math.Round(ymax * red.Y );
						int newk = (int)Math.Round(zmax * red.Z );

						if (newi % 2 != 0 || newj % 2 != 0 || newk % 2 != 0)
							continue;

						symN = retval.CalcN(newi, newj, newk);

						if (retval.Nvalues.ContainsKey(symN))
						{
							foundSym = true;

							if (symmetry.OrbitalTransform.Count > 0)
							{
								orbitals = symmetry.OrbitalTransform;
							}
						}

						if (foundSym)
							break;
					}

					if (foundSym == false && retval.Nvalues.ContainsKey(N) == false)
					{
						retval.kpts.Add(qpt);
						retval.Nvalues.Add(N, index);
						index++;
					}
					else if (retval.Nvalues.ContainsKey(N) == false)
					{
						int newIndex = retval.Nvalues[symN];
						retval.kpts[newIndex].AddOrbitalSymmetry(orbitals);

						retval.Nvalues.Add(N, newIndex);
					}
					else
					{ }  // skip points which are already in there.  This should only happen for gamma?
				}
			}

			retval.origin = points[0];
			retval.sdir = diff_1;
			retval.tdir = Vector3.CrossProduct(norm, diff_1);
			
			NormalizeST(lattice, retval);

			// now sort k-points.
			Comparison<KPoint> sorter = (x, y) =>
				{
					double s_x, s_y, t_x, t_y;

					retval.GetPlaneST(x, out s_x, out t_x);
					retval.GetPlaneST(y, out s_y, out t_y);

					if (Math.Abs(t_x - t_y) > 1e-6)
						return t_x.CompareTo(t_y);
					else
						return s_x.CompareTo(s_y);
				};

			retval.allKpts.Sort(sorter);
			retval.kpts.Sort(sorter);

			Vector3 sd = retval.sdir / SmallestNonzero(retval.sdir);
			Vector3 td = retval.tdir / SmallestNonzero(retval.tdir);

			Output.WriteLine("Plane horizontal direction: {0}", sd);
			Output.WriteLine("Plane vertical direction: {0}", td);

			return retval;
		}

		private static void NormalizeST(Lattice lattice, KptList retval)
		{
			retval.sdir /= retval.sdir.Magnitude;
			retval.tdir /= retval.tdir.Magnitude;

			retval.sdir /= GammaInDirection(lattice, retval.sdir).Magnitude;
			retval.tdir /= GammaInDirection(lattice, retval.tdir).Magnitude;

			retval.sdir *= 2;
			retval.tdir *= 2;
		}

		private static Vector3 GammaInDirection(Lattice lattice, Vector3 direction)
		{
			Vector3 sred = lattice.ReducedCoords(direction);
			sred /= SmallestNonzero(sred);

			Vector3 retval = sred.X * lattice.G1 + sred.Y * lattice.G2 + sred.Z * lattice.G3;

			return retval;
		}

		private static double SmallestNonzero(Vector3 vector3)
		{
			double smallest = double.MaxValue;
			for (int i = 0; i < 3; i++)
			{
				if (Math.Abs(vector3[i]) < smallest &&
					Math.Abs(vector3[i]) > 1e-6)
					smallest = Math.Abs(vector3[i]);
			}

			return smallest;
		}
		public void GetPlaneST(KPoint kpt, out double s, out double t)
		{
			s = (kpt.Value - origin).DotProduct(sdir);
			t = (kpt.Value - origin).DotProduct(tdir);
		}
		public static KptList GenerateMesh(Lattice lattice, int[] kgrid, int[] shift, SymmetryList syms, bool includeEnds, bool centerGamma)
		{
			KptList retval = new KptList();
			int zmax = kgrid[2] * 2;
			int ymax = kgrid[1] * 2;
			int xmax = kgrid[0] * 2;

			if (shift == null)
			{
				shift = new int[3];
			}

			retval.mesh = (int[])kgrid.Clone();
			retval.shift = (int[])shift.Clone();

			int index = 0;
			Vector3 gridVector = new Vector3(kgrid[0], kgrid[1], kgrid[2]);

			SymmetryList compatSyms = new SymmetryList();
			foreach (var symmetry in syms)
			{
				Vector3 grid2 = symmetry.Value * gridVector;
				for (int gi = 0; gi < 3; gi++)
					grid2[gi] = Math.Abs(grid2[gi]);

				if (grid2 == gridVector)
				{
					compatSyms.Add(symmetry);
				}
			}

			for (int k = 0; k <= zmax; k += 2)
			{
				for (int j = 0; j <= ymax; j += 2)
				{
					for (int i = 0; i <= xmax; i += 2)
					{
						if (includeEnds == false)
						{
							if (i == xmax || j == ymax || k == zmax)
								break;
						}

						int N = retval.CalcN(i, j, k);
						bool foundSym = false;

						double dx = (i + shift[0]) / (double)xmax;
						double dy = (j + shift[1]) / (double)ymax;
						double dz = (k + shift[2]) / (double)zmax;
						int symN = N;
						List<int> orbitals = null;

						if (centerGamma)
						{
							if (kgrid[0] > 1) dx -= 0.5;
							if (kgrid[1] > 1) dy -= 0.5;
							if (kgrid[2] > 1) dz -= 0.5;
						}
						foreach (var symmetry in compatSyms)
						{
							Vector3 pt = CalcK(lattice, dx, dy, dz);
							Vector3 Tpt = symmetry.Value * pt;

							//if (Tpt == pt)
							//    continue;

							Vector3 red = lattice.ReducedCoords(Tpt, true);

							int newi = (int)Math.Round(xmax * red.X - shift[0]);
							int newj = (int)Math.Round(ymax * red.Y - shift[1]);
							int newk = (int)Math.Round(zmax * red.Z - shift[2]);

							if (newi % 2 != 0 || newj % 2 != 0 || newk % 2 != 0)
								continue;

							symN = retval.CalcN(newi, newj, newk);

							if (symN < N)
							{
								foundSym = true;

								if (symmetry.OrbitalTransform.Count > 0)
								{
									orbitals = symmetry.OrbitalTransform;
								}
							}

							if (foundSym)
								break;
						}

						Vector3 kptValue = CalcK(lattice, dx, dy, dz);

						retval.allKpts.Add(new KPoint(kptValue));

						if (foundSym == false)
						{
							retval.kpts.Add(new KPoint(kptValue));
							retval.Nvalues.Add(N, index);
							index++;
						}
						else
						{
							int newIndex = retval.Nvalues[symN];
							retval.kpts[newIndex].Weight++;
							retval.kpts[newIndex].AddOrbitalSymmetry(orbitals);

							retval.Nvalues.Add(N, newIndex);
						}
					}
				}
			}

			int count = kgrid[0] * kgrid[1] * kgrid[2];
			for (int i = 0; i < retval.kpts.Count; i++)
			{
				retval.kpts[i].Weight /= count;
				retval.allKpts[i].Weight /= count;
			}

#if DEBUG
			if (!includeEnds)
			{
				double check = 0;
				for (int i = 0; i < retval.kpts.Count; i++)
					check += retval.kpts[i].Weight;

				System.Diagnostics.Debug.Assert(Math.Abs(check - 1) < 1e-8);
			}
#endif

			return retval;
		}


		public int GetKindex(Lattice lattice, Vector3 kpt, out List<int> orbitalMap, SymmetryList symmetries)
		{
			for (int s = 0; s < symmetries.Count; s++)
			{
				int newi, newj, newk;
				Vector3 newKpt = symmetries[s].Inverse * kpt;

				ReduceKpt(lattice, newKpt, out newi, out newj, out newk);

				int N = CalcN(newi, newj, newk);

				if (Nvalues.ContainsKey(N))
				{
					int index = Nvalues[N];

					orbitalMap = symmetries[s].OrbitalTransform;
					return index;
				}
			}

			throw new Exception(string.Format("Could not find k-point {0}", kpt));
		}

		private void ReduceKpt(Lattice lattice, Vector3 kpt, out int newi, out int newj, out int newk)
		{
			Vector3 red = lattice.ReducedCoords(kpt, true);

			int zmax = mesh[2] * 2;
			int ymax = mesh[1] * 2;
			int xmax = mesh[0] * 2;

			newi = (int)Math.Round(xmax * red.X - shift[0]);
			newj = (int)Math.Round(ymax * red.Y - shift[1]);
			newk = (int)Math.Round(zmax * red.Z - shift[2]);
		}
		private int CalcN(int i, int j, int k)
		{
			int zmax = mesh[2] * 2;
			int ymax = mesh[1] * 2;
			int xmax = mesh[0] * 2;
			int zsh = shift[2];
			int ysh = shift[1];
			int xsh = shift[0];

			return 1 + (i + xsh) + xmax * ((j + ysh) + ymax * (k + zsh));
		}
		private int CalcN(Lattice lattice, Vector3 kpt)
		{
			int i, j, k;

			ReduceKpt(lattice, kpt, out i, out j, out k);
			return CalcN(i, j, k);
		}

		public override string ToString()
		{
			return string.Format("K-points: {0}   Irreducible: {1}", allKpts.Count, kpts.Count);
		}
	}
}