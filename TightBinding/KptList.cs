
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
		public static KptList GenerateMesh(Lattice L, int x, int y, int z)
		{
			KptList retval = new KptList();
			
			double xx = 1 / (double)x, 
				yy = 1 / (double)y, 
				zz = 1 / (double)z;

			for (int i = 0; i < x; i++)
			{
				double dx = i * xx;

				for (int j = 0; j < y; j++)
				{
					double dy = j * yy;

					for (int k = 0; k < z; k++)
					{
						double dz = k * zz;

						retval.kpts.Add(new KPoint(CalcK(L, dx, dy, dz)));

						// See J. Phys.: Condens. Matter 2 (1990) 7445-7452
						// page 2 contains numbering of corners
						// Using scheme S2.
						// x axis is 1-2, y axis is 1-4, z axis is 1-7.
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        // 1238
							CalcK(L, dx + xx, dy, dz),
							CalcK(L, dx+xx, dy+yy, dz),
							CalcK(L, dx+xx, dy, dz+zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1345
							CalcK(L, dx+xx, dy+yy, dz),
							CalcK(L, dx, dy+yy, dz),
							CalcK(L, dx+xx, dy+yy, dz+zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1358
							CalcK(L, dx+xx, dy+yy, dz),
							CalcK(L, dx+xx, dy+yy, dz+zz),
							CalcK(L, dx+xx, dy, dz+zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1457
							CalcK(L, dx, dy+yy, dz),
							CalcK(L, dx+xx, dy+yy, dz+zz),
							CalcK(L, dx, dy, dz+zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy, dz),        //1578
							CalcK(L, dx+xx, dy+yy, dz+zz),
							CalcK(L, dx, dy, dz+zz),
							CalcK(L, dx+xx, dy, dz+zz)));
						retval.tets.Add(new Tetrahedron(
							CalcK(L, dx, dy+yy, dz),        //4567
							CalcK(L, dx+xx, dy+yy, dz+zz),
							CalcK(L, dx, dy+yy, dz+zz),
							CalcK(L, dx, dy, dz+zz)));
					}
				}
			}
			
			return retval;
		}
		public static Vector3 CalcK(Lattice l, double dx, double dy, double dz)
		{
			return -2 * Math.PI * (dx * l.G1 + dy * l.G2 + dz * l.G3);
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

	}
}
