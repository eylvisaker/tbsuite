
using System;
using System.Collections.Generic;
using System.IO;
using ERY.EMath;

namespace TightBindingSuite
{
	public class Lattice
	{
		Vector3 a1, a2, a3;
		Vector3 g1, g2, g3;
		
		public Lattice(Vector3 a1, Vector3 a2, Vector3 a3)
		{
			this.a1 = a1;
			this.a2 = a2;
			this.a3 = a3;
			
			CalculateReciprocalVectors();
		}

		internal Lattice Clone()
		{
			Lattice retval = new Lattice(a1, a2, a3);
			return retval;
		}

		void CalculateReciprocalVectors()
		{
			Matrix m = new Matrix(3, 3);
			
			m.SetRows(a1, a2, a3);
			
			m = m.InvertByRowOperations();
			
			g1 = m.GetColumnAsVector3(0);
			g2 = m.GetColumnAsVector3(1);
			g3 = m.GetColumnAsVector3(2);
		}
		
		public Vector3 A1 { get { return a1; }}
		public Vector3 A2 { get { return a2; }}
		public Vector3 A3 { get { return a3; }}
		public Vector3 G1 { get { return g1; }}
		public Vector3 G2 { get { return g2; }}
		public Vector3 G3 { get { return g3; }}
		
		public Vector3 ReducedCoords(Vector3 a)
		{
			return ReducedCoords(a, false);	
		}
		public Vector3 ReducedCoords(Vector3 a, bool translate)
		{
			Matrix m = new Matrix(3,3);
			
			m.SetRow(0, a1);
			m.SetRow(1, a2);
			m.SetRow(2, a3);
			
			Vector3 retval = m * a;

			if (translate)
			{
				while (retval.X < -0.5) retval.X += 1;
				while (retval.Y < -0.5) retval.Y += 1;
				while (retval.Z < -0.5) retval.Z += 1;

				while (retval.X > 0.5) retval.X -= 1;
				while (retval.Y > 0.5) retval.Y -= 1;
				while (retval.Z > 0.5) retval.Z -= 1;
			}

			return retval;
		}

	}
}
