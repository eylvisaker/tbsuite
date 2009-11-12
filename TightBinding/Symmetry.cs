using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class Symmetry
	{
		Matrix mInverse;

		public Symmetry(Matrix val)
		{
			Value = val;
			OrbitalTransform = new List<int>();
		}

		public Matrix Value { get; private set; }
		public List<int> OrbitalTransform { get; private set; }

		public Matrix Inverse
		{
			get
			{
				if (mInverse == null)
					mInverse = Value.Invert();

				return mInverse;
			}
		}

		public Symmetry Clone()
		{
			Symmetry x = new Symmetry(Value.Clone());
			x.OrbitalTransform.AddRange(OrbitalTransform);

			return x;
		}
	}
}
