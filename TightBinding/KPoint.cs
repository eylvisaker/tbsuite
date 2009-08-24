
using System;
using ERY.EMath;

namespace TightBinding
{
	
	
	public class KPoint
	{
		public Vector3 Value;
		public double Weight = 1;
		public string Name;
		
		public KPoint(Vector3 v)
		{
			Value = v;	
		}
		public static implicit  operator Vector3(KPoint p)
		{
			return p.Value;	
		}
	}
}
