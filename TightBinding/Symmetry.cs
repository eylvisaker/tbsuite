using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
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
	}
}
