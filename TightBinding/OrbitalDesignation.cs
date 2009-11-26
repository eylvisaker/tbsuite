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
		public static OrbitalDesignation TransformUnderSymmetry(OrbitalDesignation des, Matrix sym)
		{
			switch(des)
			{
				case OrbitalDesignation.s: 
					return OrbitalDesignation.s;

				case OrbitalDesignation.px:
					return TransformP(sym[0, 0], sym[0, 1], sym[0, 2]);
				case OrbitalDesignation.py:
					return TransformP(sym[1, 0], sym[1, 1], sym[1, 2]);
				case OrbitalDesignation.pz:
					return TransformP(sym[2, 0], sym[2, 1], sym[2, 2]);

			}

			return OrbitalDesignation.none;
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
