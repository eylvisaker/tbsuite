using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace RPA
{
	public class Wavefunction
	{
		Complex[] coeffs;
		double energy;
		double fermiFunction;

		public Wavefunction(int basisSize)
		{
			coeffs = new Complex[basisSize];
		}

		public Complex[] Coeffs
		{
			get { return coeffs; }
		}
		public double Energy
		{
			get { return energy; }
			set { energy = value; }
		}
		public double FermiFunction
		{
			get { return fermiFunction; }
			set { fermiFunction = value; }
		}
	}
}
