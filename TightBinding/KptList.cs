
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBinding
{
	public class KptList
	{
		List<KPoint>  kpts = new List<KPoint>();
		List<Tetrahedron> tets = new List<Tetrahedron>();

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
		public List<Tetrahedron> Tetrahedrons { get { return tets; } }

		public static KptList GenerateMesh(Lattice L, int x, int y, int z)
		{
			KptList retval = new KptList();

			double xx = 1 / (double)x,
				yy = 1 / (double)y,
				zz = 1 / (double)z;

			for (int k = 0; k < z; k++)
			{
				double dz = k * zz;
				
				for (int j = 0; j < y; j++)
				{
					double dy = j * yy;

					for (int i = 0; i < x; i++)
					{
						double dx = i * xx;

						retval.kpts.Add(new KPoint(CalcK(L, dx, dy, dz)));

						// See J. Phys.: Condens. Matter 2 (1990) 7445-7452
						// page 2 contains numbering of corners
						// Using scheme S2.
						// x axis is 1-2, y axis is 1-4, z axis is 1-7.
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        // 1238
							CalcK(L, dx + xx, dy, dz),
							CalcK(L, dx + xx, dy + yy, dz),
							CalcK(L, dx + xx, dy, dz + zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1345
							CalcK(L, dx + xx, dy + yy, dz),
							CalcK(L, dx, dy + yy, dz),
							CalcK(L, dx + xx, dy + yy, dz + zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1358
							CalcK(L, dx + xx, dy + yy, dz),
							CalcK(L, dx + xx, dy + yy, dz + zz),
							CalcK(L, dx + xx, dy, dz + zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1457
							CalcK(L, dx, dy + yy, dz),
							CalcK(L, dx + xx, dy + yy, dz + zz),
							CalcK(L, dx, dy, dz + zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1578
							CalcK(L, dx + xx, dy + yy, dz + zz),
							CalcK(L, dx, dy, dz + zz),
							CalcK(L, dx + xx, dy, dz + zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy + yy, dz),        //4567
							CalcK(L, dx + xx, dy + yy, dz + zz),
							CalcK(L, dx, dy + yy, dz + zz),
							CalcK(L, dx, dy, dz + zz)));
					}
				}
			}

			return retval;
		}

		internal static KptList GenerateMesh(Lattice lattice, int[] kgrid, int[] shift, SymmetryList syms)
		{
			KptList retval = new KptList();
			int zmax = kgrid[2] * 2;
			int ymax = kgrid[1] * 2;
			int xmax = kgrid[0] * 2;

			Dictionary<int, int> values = new Dictionary<int, int>();
			int index = 0;

			for (int k = 0; k < zmax; k+=2)
			{
				for (int j = 0; j < ymax; j+=2)
				{
					for (int i = 0; i < xmax; i += 2)
					{
						int N = CalcN(kgrid, shift, i, j, k);
						bool foundSym = false;

						double dx = (i + shift[0]) / (double)xmax;
						double dy = (j + shift[1]) / (double)ymax;
						double dz = (k + shift[2]) / (double)zmax;
						int symN = N;
						List<int> orbitals = null;

						foreach (var symmetry in syms)
						{
							if (symmetry.Value.IsIdentity)
								continue;

							Vector3 pt = CalcK(lattice, dx, dy, dz);
							Vector3 Tpt = symmetry.Value * pt;

							if (Tpt == pt)
								continue;

							Vector3 red = lattice.ReducedCoords(Tpt, true);

							int newi = (int)Math.Round(xmax * red.X - shift[0]);
							int newj = (int)Math.Round(ymax * red.Y - shift[1]);
							int newk = (int)Math.Round(zmax * red.Z - shift[2]);

							symN = CalcN(kgrid, shift, newi, newj, newk);

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

						if (foundSym == false)
						{
							retval.kpts.Add(new KPoint(CalcK(lattice, dx, dy, dz)));
							values.Add(N, index);
							index++;
						}
						else
						{
							int newIndex = values[symN];
							retval.kpts[newIndex].Weight++;
							retval.kpts[newIndex].SetOrbitalSymmetry(orbitals);

							values.Add(N, newIndex);

						}
					}
				}
			}

			int count = kgrid[0] * kgrid[1] * kgrid[2];
			for (int i = 0; i < retval.kpts.Count; i++)
				retval.kpts[i].Weight /= count;

#if DEBUG
			double check = 0;
			for (int i = 0; i < retval.kpts.Count; i++)
				check += retval.kpts[i].Weight;

			System.Diagnostics.Debug.Assert(Math.Abs(check - 1) < 1e-8);
#endif
			Console.WriteLine("Applied {0} symmetries to get {1} irreducible kpoints from {2}.",
				syms.Count, retval.kpts.Count, count);

			return retval;
		}

		private static int CalcN(int[] kgrid, int[] shift, int i, int j, int k)
		{
			int zmax = kgrid[2] * 2;
			int ymax = kgrid[1] * 2;
			int xmax = kgrid[0] * 2;
			int zsh = shift[2];
			int ysh = shift[1];
			int xsh = shift[0];

			return 1 + (i + xsh) + xmax * ((j + ysh) + ymax * (k + zsh));
		}
	}
}
