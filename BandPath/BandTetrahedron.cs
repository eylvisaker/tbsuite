using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	public class BandTetrahedron
	{
		public BandTetrahedron (TightBinding tb, Vector3 anchor, KptList kpts, List<int> indices)
		{
			for (int i = 0; i < indices.Count; i++)
			{
				KPoint kpt = kpts.Kpts[indices[i]].Clone();
				
				Vector3 delta = kpt.Value - anchor;
				
				ShiftDelta(ref delta, tb.Lattice.G1);
				ShiftDelta(ref delta, tb.Lattice.G2);
				ShiftDelta(ref delta, tb.Lattice.G3);
				
				kpt.Value = delta + anchor;
				
				this.kpts.Add(kpt);
			}
			
			CalculateVelocityMatrix();
		}
		
		void CalculateVelocityMatrix()
		{
			Matrix m = new Matrix(3,3);
			
			for (int i = 1; i <= 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					m[i-1, j] = kpts[i].Value[j] - kpts[0].Value[j];
				}
			}
			
			velocity = m.Invert();
		}
		
		public double Interpolate(KPoint kpt)
		{
		
			return 0;
		}
		void ShiftDelta(ref Vector3 delta, Vector3 G)
		{
			if ((delta - G).Magnitude < delta.Magnitude)
			{
				delta -= G;	
			}
			if ((delta + G).Magnitude < delta.Magnitude)
			{
				delta += G;	
			}
		}
		
		List<KPoint> kpts = new List<KPoint>();
		Matrix velocity;
		
		public bool Contains(KPoint kpt)
		{
			return false;
		}
	}
}

