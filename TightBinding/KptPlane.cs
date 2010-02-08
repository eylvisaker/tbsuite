using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class KptPlane : KptList 
	{
		Vector3 sdir, tdir, origin;
		Vector3[] G = new Vector3[3];
		Lattice lattice;

		public new KptPlane Clone()
		{
			return (KptPlane)CloneImpl();
		}
		protected override KptList CloneImpl()
		{
			KptPlane retval = new KptPlane();

			base.CloneBaseImpl(retval);

			retval.sdir = sdir;
			retval.tdir = tdir;
			retval.origin = origin;

			return retval;
		}

		public Vector3 Origin { get { return origin; } protected set { origin = value; } }
		public Vector3 Sdir { get { return sdir; } protected set { sdir = value; } }
		public Vector3 Tdir { get { return tdir; } protected set { tdir = value; } }

		[Browsable(false)]
		public override KptList  CreateIrreducibleMesh(SymmetryList symmetryList)
		{
			return CreateIrreduciblePlane(symmetryList);
		}

		public KptPlane CreateIrreduciblePlane(SymmetryList symmetries)
		{
			KptPlane p = new KptPlane();

			p.origin = origin;
			p.tdir = tdir;
			p.sdir = sdir;

			FillIrreducibleMesh(symmetries, p);

			return p;
		}

		public static KptPlane GeneratePlane(Lattice lattice, Vector3[] points, int[] qgrid, int[] qshift)
		{
			KptList qmesh = GenerateMesh(qgrid, qshift, true);
			return GeneratePlane(lattice, points, qmesh);
		}
		public static KptPlane GeneratePlane(Lattice lattice, Vector3[] points, KptList qmesh)
		{
			Vector3 diff_1 = points[1] - points[0];
			Vector3 diff_2 = points[2] - points[0];
			Vector3 norm = Vector3.CrossProduct(diff_1, diff_2);

			KptPlane retval = new KptPlane();

			retval.SetLattice(lattice);

			retval.mesh = (int[])qmesh.Mesh.Clone();
			retval.shift = new int[3];

			retval.origin = points[0];
			retval.sdir = diff_1;
			retval.tdir = diff_2;

			NormalizeST(lattice, retval);

			int zmax = qmesh.Mesh[2] * 2;
			int ymax = qmesh.Mesh[1] * 2;
			int xmax = qmesh.Mesh[0] * 2;

			List<KPoint> planePoints = new List<KPoint>();
			for (int i = 0; i < qmesh.Kpts.Count; i++)
			{
				var qpt = qmesh.Kpts[i];

				Vector3 diff = qpt.Value - points[0];
				double dot = Math.Abs(diff.DotProduct(norm));

				if (dot < 1e-8)
				{
					double s, t;

					retval.GetPlaneST(qpt, out s, out t);
					retval.AddKpt(qpt);
				}
			}
			
			// now sort q-points to lay them in the s,t plane.
			retval.SortKpoints();

			Vector3 sd = retval.sdir / SmallestNonzero(retval.sdir);
			Vector3 td = retval.tdir / SmallestNonzero(retval.tdir);

			Output.WriteLine("Plane horizontal direction: {0}", sd);
			Output.WriteLine("Plane vertical direction: {0}", td);

			Output.WriteLine("Plane horizontal vector: {0}", retval.sdir);
			Output.WriteLine("Plane vertical vector: {0}", retval.tdir);

			return retval;
		}

		protected void SetLattice(Lattice lattice)
		{
			this.lattice = lattice;
			this.G[0] = lattice.G1;
			this.G[1] = lattice.G2;
			this.G[2] = lattice.G3;
		}

		protected void SortKpoints()
		{
			Comparison<KPoint> sorter = (x, y) =>
			{
				double s_x, s_y, t_x, t_y;

				GetPlaneST(x, out s_x, out t_x);
				GetPlaneST(y, out s_y, out t_y);

				if (Math.Abs(t_x - t_y) > 1e-6)
					return t_x.CompareTo(t_y);
				else
					return s_x.CompareTo(s_y);
			};

			Kpts.Sort(sorter);
		}

		private static void NormalizeST(Lattice lattice, KptPlane retval)
		{
			retval.sdir /= retval.sdir.Magnitude;
			retval.tdir /= retval.tdir.Magnitude;

			retval.sdir /= GammaInDirection(lattice, retval.sdir).DotProduct(retval.sdir);
			retval.tdir /= GammaInDirection(lattice, retval.tdir).DotProduct(retval.tdir);

			// double them to make s and t 1 at the zone boundary, instead of 0.5.
			retval.sdir *= 2;
			retval.tdir *= 2;
		}

		private static void SortByDistanceFromGamma(List<KPoint> planePoints)
		{
			planePoints.Sort(
				(x, y) =>
				{
					return x.Value.Magnitude.CompareTo(y.Value.Magnitude);
				});
		}

		private static Vector3 GammaInDirection(Lattice lattice, Vector3 direction)
		{
			Vector3 sred = lattice.ReciprocalReduce(direction);
			sred /= LargestNonzero(sred);

			Vector3 retval = sred.X * lattice.G1 + sred.Y * lattice.G2 + sred.Z * lattice.G3;

			return retval;
		}

		private static double LargestNonzero(Vector3 vector3)
		{
			double largest = 0;
			for (int i = 0; i < 3; i++)
			{
				largest = Math.Max(largest, Math.Abs(vector3[i]));
			}

			return largest;
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

		public bool GetPlaneST(KPoint kpt, out double s, out double t)
		{
			return GetPlaneST(kpt.Value, out s, out t);
		}
		public bool GetPlaneST(Vector3 kpt, out double s, out double t)
		{
			Vector3 v = new Vector3();

			// expand to cartesian coordinates
			for (int i = 0; i < 3; i++)
			{
				v += (kpt[i] - origin[i]) * G[i];
			}
		
			s = v.DotProduct(sdir);
			t = v.DotProduct(tdir);

			double norm = (kpt - origin).DotProduct(sdir.CrossProduct(tdir));

			if (Math.Abs(norm) > 1e-7)
				return false;
			else
				return true;
		}


		public Vector3 ReduceST(double s, double t)
		{
			Vector3 v = new Vector3();

			v = s * sdir;
			v += t * tdir;
			v += origin;

			return lattice.ReciprocalReduce(v);
		}
	}
}
