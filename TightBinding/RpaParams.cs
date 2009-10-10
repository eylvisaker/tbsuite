using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
{
	public class RpaParams
	{
		public RpaParams(int qptIndex, double temperature, double frequency, double mu)
		{
			Qindex = qptIndex;
			Temperature = temperature;
			Frequency = frequency;
			ChemicalPotential = mu;
		}

		public int Qindex { get; private set; }
		public double Temperature { get; private set; }
		public double Frequency { get; private set; }
		public double ChemicalPotential { get; private set; }

		public Matrix X0 { get; set; }
		public Matrix Xs { get; set; }
		public Matrix Xc { get; set; }


		/// <summary>
		/// Sorts by qindex, chemical potential, frequency, and temperature
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static int TemperatureComparison(RpaParams x, RpaParams y)
		{
			if (x.Qindex != y.Qindex)
				return x.Qindex.CompareTo(y.Qindex);

			if (x.ChemicalPotential != y.ChemicalPotential)
				return x.ChemicalPotential.CompareTo(y.ChemicalPotential);

			if (x.Frequency != y.Frequency)
				return x.Frequency.CompareTo(y.Frequency);

			return x.Temperature.CompareTo(y.Temperature);
		}
		/// <summary>
		/// Sorts by temperature, chemical potential, frequency, qindex.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static int QIndexComparison(RpaParams x, RpaParams y)
		{
			if (x.Temperature != y.Temperature)
				return x.Temperature.CompareTo(y.Temperature);

			if (x.ChemicalPotential != y.ChemicalPotential)
				return x.ChemicalPotential.CompareTo(y.ChemicalPotential);

			if (x.Frequency != y.Frequency)
				return x.Frequency.CompareTo(y.Frequency);

			return x.Qindex.CompareTo(y.Qindex);
		}
	}
}
