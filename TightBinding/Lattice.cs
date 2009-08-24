
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBinding
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
		
		void CalculateReciprocalVectors()
		{
			Matrix m = new Matrix(3, 3);
			
			m.SetRows(a1, a2, a3);
			
			m = m.InvertByRowOperations();
			
			g1 = m.GetColumnAsVector3(0);
			g2 = m.GetColumnAsVector3(1);
			g3 = m.GetColumnAsVector3(2);
			
			Console.WriteLine("a1: {0}", a1);
			Console.WriteLine("a2: {0}", a2);
			Console.WriteLine("a3: {0}", a3);
			Console.WriteLine("g1: {0}", g1);
			Console.WriteLine("g2: {0}", g2);
			Console.WriteLine("g3: {0}", g3);
			
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
			
			m.SetRow(0, g1);
			m.SetRow(1, g2);
			m.SetRow(2, g3);
			
			Vector3 retval = m * a;
			
			if (retval.X < 0) retval.X += (int)retval.X + 1;
			if (retval.Y < 0) retval.Y += (int)retval.Y + 1;
			if (retval.Z < 0) retval.Z += (int)retval.Z + 1;
			
			retval.X %= 1;
			retval.Y %= 1;
			retval.Z %= 1;
			
			return retval;
		}
	}
}
