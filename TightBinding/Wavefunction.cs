using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
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

		public Wavefunction Clone(List<int> orbitalMap)
		{
			Wavefunction retval = new Wavefunction(coeffs.Length);

			if (orbitalMap != null && orbitalMap.Count > 0)
			{
				for (int i = 0; i < coeffs.Length; i++)
				{
					retval.coeffs[orbitalMap[i]] = coeffs[i];
				}
			}
			else
				retval.coeffs = (Complex[])coeffs.Clone();

			return retval;
		}

	}
}
