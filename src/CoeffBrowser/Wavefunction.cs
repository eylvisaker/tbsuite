using System;
using System.Collections.Generic;
using ERY.EMath;

namespace CoeffBrowser
{
	public class Wavefunction
	{
		public Wavefunction ()
		{
		}
		
		public List<Complex> Coeffs = new List<Complex>();
		public double Energy { get; set; }
	}
}
