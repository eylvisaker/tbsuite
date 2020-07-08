using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class BZonePlane : KptPlane
	{
		class BZoneKPoint : KPoint
		{
			public BZoneKPoint(KPoint value, int index)
				: base(value.Value)
			{
				TargetIndex = index;
			}
			public BZoneKPoint(Vector3 value)
				: base(value)
			{
				TargetIndex = -1;
			}

			public int TargetIndex { get; set; }
		}

		public static BZonePlane CreateBZonePlane(KptPlane plane, Lattice lattice)
		{
			return new BZonePlane(plane, lattice);
		}

		private BZonePlane(KptPlane plane, Lattice lattice)
		{
			SetLattice(lattice);
			Origin = plane.Origin;
			Sdir = plane.Sdir;
			Tdir = plane.Tdir;

			List<Vector3> nearbyGammas = NearbyGammas();

			double lastT = double.MinValue;

			for (int i = 0; i < plane.Kpts.Count; i++)
			{
				var kpt = plane.Kpts[i];

				this.Kpts.Add(new BZoneKPoint(kpt, i));

				double s, t;
				plane.GetPlaneST(kpt, out s, out t);

				if (t != lastT)
				{
					lastT = t;

					Vector3 closestGamma = ClosestGamma(nearbyGammas, kpt, lattice);
					AddLeftBoundary(closestGamma, kpt, plane, lattice);
				}

			}

			SortKpoints();
		}

		private void AddLeftBoundary(Vector3 closestGamma, KPoint kpt, KptPlane plane, Lattice lattice)
		{
			double s, t;
			plane.GetPlaneST(kpt, out s, out t);

			double gs, gt;
			plane.GetPlaneST(closestGamma, out gs, out gt);

			if (gs == 0)
				return;

			double ratio = gt / gs;

			// this is the solution to
			// |S| = |S-P| where S is the target point, P is the nearby Gamma point
			// and the y component of S is constrained to be the same as for the input.
			double news = 0.5 * (gs + gt * ratio - 2 * t * ratio);

			if (Math.Abs(news - s) < 1e-6)
				return;

			Kpts.Add(new BZoneKPoint(plane.ReduceST(news, t)));

		}

		private Vector3 ClosestGamma(List<Vector3> nearbyGammas, KPoint kpt, Lattice lattice)
		{
			if (nearbyGammas.Count == 0)
				throw new ArgumentException();

			double minDistance = double.MaxValue;
			Vector3 closest = new Vector3();

			Vector3 kptCart = lattice.ReciprocalExpand(kpt.Value);

			foreach (var pt in nearbyGammas)
			{
				Vector3 gamma = lattice.ReciprocalExpand(pt);

				double distance = (kptCart - gamma).Magnitude;
				double s, t;
				GetPlaneST(pt, out s, out t);

				if (distance < minDistance)
				{
					minDistance = distance;
					closest = pt;
				}
			}

			return closest;
		}

		private static List<Vector3> NearbyGammas()
		{
			List<Vector3> nearbyGammas = new List<Vector3>();

			for (int k = -1; k <= 1; k++)
			{
				for (int j = -1; j <= 1; j++)
				{
					for (int i = -1; i <= 1; i++)
					{
						if (i == 0 && j == 0 && k == 0)
							continue;

						Vector3 p = new Vector3(i, j, k);

						nearbyGammas.Add(p);
					}
				}
			}

			return nearbyGammas;
		}

		public Matrix GetValue(int index, KptPlane plane, MatrixValueGetter getter, params object[] args)
		{
			BZoneKPoint kpt = (BZoneKPoint)base.Kpts[index];

			if (kpt.TargetIndex != -1)
			{
				return getter(kpt.TargetIndex, args);
			}
			else
				return Matrix.Zero(4);

		}
	}

	public delegate Matrix MatrixValueGetter(int index, object[] parameters);
}
