
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace CoeffBrowser
{


	public class KPoint
	{

		public KPoint ()
		{
		}
		
		
		public int Spin { get; set; }
		public Vector3 K { get; set; }
		public double dk { get; set; }
		
		public List<Wavefunction> Wfk = new List<Wavefunction>();
	}
}
