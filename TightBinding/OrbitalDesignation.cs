using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public enum OrbitalDesignation
	{
		none,

		s,
		py,
		pz,
		px,
		dxy,
		dyz,
		dz2,
		dxz,
		dx2y2,
	}

	public static class ODHelper
	{

		internal static OrbitalDesignation FromString(string p)
		{
			switch (p)
			{
				case "s": return OrbitalDesignation.s;
				case "py": return OrbitalDesignation.py;
				case "pz": return OrbitalDesignation.pz;
				case "px": return OrbitalDesignation.px;
				case "dxy": return OrbitalDesignation.dxy;
				case "dyz": return OrbitalDesignation.dyz;
				case "dz2": return OrbitalDesignation.dz2;
				case "dxz": return OrbitalDesignation.dxz;
				case "dx2y2": return OrbitalDesignation.dx2y2;

				case "d-2": return OrbitalDesignation.dxy;
				case "d-1": return OrbitalDesignation.dyz;

				case "d+0":
				case "d0": return OrbitalDesignation.dz2;

				case "d+1":
				case "d1": return OrbitalDesignation.dxz;

				case "d+2":
				case "d2": return OrbitalDesignation.dx2y2;
			}

			throw new Exception(string.Format(
				"Could not understand symmetry designation {0}.", p));
		}

		public static OrbitalDesignation TransformUnderSymmetry(OrbitalDesignation des, Matrix sym)
		{
			switch (des)
			{
				case OrbitalDesignation.s:
					return OrbitalDesignation.s;

				case OrbitalDesignation.px: return TransformP(sym[0, 0], sym[0, 1], sym[0, 2]);
				case OrbitalDesignation.py: return TransformP(sym[1, 0], sym[1, 1], sym[1, 2]);
				case OrbitalDesignation.pz: return TransformP(sym[2, 0], sym[2, 1], sym[2, 2]);

				case OrbitalDesignation.dxy:
					return TransformT2g(sym, 0, 1);
				case OrbitalDesignation.dxz:
					return TransformT2g(sym, 0, 2);
				case OrbitalDesignation.dyz:
					return TransformT2g(sym, 1, 2);
				case OrbitalDesignation.dz2:
					return TransformDz2(sym);
				case OrbitalDesignation.dx2y2:
					return TransformDx2y2(sym);
			}

			return OrbitalDesignation.none;
		}

		private static OrbitalDesignation TransformDx2y2(Matrix sym)
		{
			if (sym.SubMatrix(0, 0, 2, 2).IsDiagonal)
				return OrbitalDesignation.dx2y2;

			return OrbitalDesignation.none;
		}

		private static OrbitalDesignation TransformDz2(Matrix sym)
		{
			if (IsZero(sym[0, 2]) && IsZero(sym[1, 2]) && IsZero(sym[2, 0]) && IsZero(sym[2, 1]))
				return OrbitalDesignation.dz2;
			else
				return OrbitalDesignation.none;
		}

		private static bool IsZero(Complex complex)
		{
			return Math.Abs(complex.Magnitude) < 1e-6;
		}

		private static OrbitalDesignation TransformT2g(Matrix sym, int a, int b)
		{
			Complex x = sym[0, a] + sym[0, b];
			Complex y = sym[1, a] + sym[1, b];
			Complex z = sym[2, a] + sym[2, b];

			double xx = x.Magnitude;
			double yy = y.Magnitude;
			double zz = z.Magnitude;

			if (IsOne(xx) && IsOne(yy)) return OrbitalDesignation.dxy;
			if (IsOne(xx) && IsOne(zz)) return OrbitalDesignation.dxz;
			if (IsOne(yy) && IsOne(zz)) return OrbitalDesignation.dyz;

			return OrbitalDesignation.none;
		}

		private static bool IsOne(double yy)
		{
			return Math.Abs(yy - 1) < 1e-6;
		}

		private static OrbitalDesignation TransformP(params Complex [] m)
		{
			if (m.Length != 3)
				throw new ArgumentException();

			int nonzeroCount = 0;

			for (int i = 0; i < 3; i++)
			{
				if (m[i].Magnitude > 1e-8)
					nonzeroCount++;
			}

			if (nonzeroCount != 1)
				return OrbitalDesignation.none;

			if (m[0].Magnitude > 1e-8) return OrbitalDesignation.px;
			if (m[1].Magnitude > 1e-8) return OrbitalDesignation.py;
			if (m[2].Magnitude > 1e-8) return OrbitalDesignation.pz;

			return OrbitalDesignation.none;
		}

	}
}
