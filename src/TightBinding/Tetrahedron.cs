using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class Tetrahedron
	{
		static double[,] Kronecker = new double[4, 4];

		static Tetrahedron()
		{
			for (int i = 0; i < 4; i++)
				Kronecker[i, i] = 1;
		}

		Vector3[] corners = new Vector3[4];
		double[] values = new double[4];

		public Tetrahedron() { }
		public Tetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			corners[0] = a;
			corners[1] = b;
			corners[2] = c;
			corners[3] = d;
		}

		public Vector3[] Corners { get { return corners; } }
		/// <summary>
		/// Values of corners used in integration.
		/// If you don't know what's there, it's undefined.
		/// </summary>
		public double[] Values { get { return values; } }

		public void SortCorners()
		{
			int i = 1;
			while (i < 4)
			{
				if (values[i] < values[i - 1])
				{
					Swap(ref corners[i], ref corners[i - 1]);
					Swap(ref values[i], ref values[i - 1]);

					if (i > 1)
						i--;
				}
				else
					i++;
			}
		}

		void Swap<T>(ref T a, ref T b)
		{
			T t = a;
			a = b;
			b = t;
		}

		public double IntegrateVolume(double ef)
		{
			// see J. Phys.: Condens. Matter 2 (1990) 7445-7452
			// for relevant equations.
			if (ef < values[0]) return 0;
			if (ef > values[3])
			{
				return 1;
			}
			
			double[] c = CalcVolumeCoefficients(ef);

			double retval = 0;

			for (int j = 0; j < 4; j++)
			{
				retval += c[j];
			}
			
			retval /= 4;

			System.Diagnostics.Debug.Assert(
				double.IsNaN(retval) == false && double.IsInfinity(retval) == false);

			return retval;
		}

		private double[] CalcVolumeCoefficients(double ef)
		{
			this.ef = ef;

			double[] c = new double[4];

			for (int j = 0; j < 4; j++)
			{
				if (ef < values[1])
				{
					c[j] = 4 * Kronecker[j, 0];
					for (int m = 1; m < 4; m++)
					{
						c[j] += (Kronecker[m, j] - Kronecker[j, 0]) *
							Delta(m, 0);
					}
					c[j] *= Delta(1, 0) * Delta(2, 0) * Delta(3, 0);
				}
				else if (ef < values[2])
				{
					double a = (1 + (Kronecker[j, 0] - Kronecker[j, 3]) * Delta(0, 3)) * Delta(3, 0);
					double b = (Kronecker[j, 2] + Delta(j, 3 - j) + (Kronecker[j, 0] + Kronecker[j, 2]) * Delta(j, 3 - j)) * Delta(0, 2) * Delta(1, 2) * Delta(3, 0);
					double cc = (Kronecker[j, 1] + Delta(j, 3 - j) + (Kronecker[j, 1] + Kronecker[j, 3]) * Delta(j, 3 - j)) * Delta(0, 3) * Delta(2, 1) * Delta(3, 1);

					c[j] = a - b + cc;
				}
				else if (ef < values[3])
				{
					c[j] = 4 * Kronecker[j, 3];
					for (int m = 0; m < 3; m++)
					{
						c[j] += (Kronecker[m, j] - Kronecker[j, 3]) *
							Delta(m, 3);
					}
					c[j] *= Delta(0, 3) * Delta(1, 3) * Delta(2, 3);
					c[j] = 1 - c[j];
				}
			}
			return c;
		}
		double ef;
		double Delta( int j, int jprime)
		{
			return (ef - values[jprime]) / (values[j] - values[jprime]);
		}

		public double IntegrateAreaNumeric(double ef)
		{
			double delta = 0.000001;
			double fpd = IntegrateVolume(ef + delta);
			double fmd = IntegrateVolume(ef - delta);

			double retval = 0.5 * (fpd - fmd) / delta;

			return retval;
		}
		public double IntegrateArea(double ef)
		{
			// see J. Phys.: Condens. Matter 2 (1990) 7445-7452
			// for relevant equations.
			if (ef < values[0]) return 0;
			if (ef > values[3]) return 0;

			double[] c = CalcAreaCoeffs(ef);

			double retval = c.Sum() / 4;

			double delta = 0.000001;
			double[] vcpd = CalcVolumeCoefficients(ef + delta);
			double[] vcmd = CalcVolumeCoefficients(ef - delta);
			double[] vcd = new double[4];

			for (int i = 0; i < 4; i++)
			{
				vcd[i] = (vcpd[i] - vcmd[i]) / (2 * delta);
			}
			double r = vcd.Sum() / 4;

			System.Diagnostics.Debug.Assert(
				double.IsNaN(retval) == false && double.IsInfinity(retval) == false);

			return retval;
		}

		private double[] CalcAreaCoeffs(double ef)
		{
			this.ef = ef;
			double[] c = new double[4];

			for (int j = 0; j < 4; j++)
			{
				if (ef < values[1])
				{
					if (j == 0)
					{
						c[j] = 
							diffDelta(1,0) * Delta(2,0) * Delta(3,0) +
							Delta(1,0) * diffDelta(2,0) * Delta(3,0) +
							Delta(1,0) * Delta(2,0) * diffDelta(3,0);
						c[j] *= 4;
					}

					for (int m = 1; m < 4; m++)
					{
						c[j] += (Kronecker[m, j] - Kronecker[j, 0]) *
							DiffDeltaFour(m, 0);
					}
				}
				else if (ef < values[2])
				{
					double a = diffDelta(3, 0) + (Kronecker[j, 0] - Kronecker[j, 3]) *
						(Delta(0, 3) * diffDelta(3, 0) + diffDelta(0, 3) * Delta(3, 0));

					double bDiff =
						diffDelta(0, 2) * Delta(1, 2) * Delta(3, 0) +
						Delta(0, 2) * diffDelta(1, 2) * Delta(3, 0) +
						Delta(0, 2) * Delta(1, 2) * diffDelta(3, 0);
					double bNoDiff = Delta(0, 2) * Delta(1, 2) * Delta(3, 0);

					double b =
						Kronecker[j, 2] * bDiff +
							Delta(j, 3 - j) * bDiff +
							diffDelta(j, 3 - j) * bNoDiff +
						(Kronecker[j, 0] + Kronecker[j, 2]) *
						(Delta(j, 3 - j) * bDiff + diffDelta(j, 3 - j) * bNoDiff);

					double cDiff =
						diffDelta(0, 3) * Delta(2, 1) * Delta(3, 1) +
						Delta(0, 3) * diffDelta(2, 1) * Delta(3, 1) +
						Delta(0, 3) * Delta(2, 1) * diffDelta(3, 1);
					double cNoDiff = Delta(0, 3) * Delta(2, 1) * Delta(3, 1);

					double cc = Kronecker[j, 1] * cDiff +
						Delta(j, 3 - j) * cDiff +
						diffDelta(j, 3 - j) * cNoDiff +
						(Kronecker[j, 1] + Kronecker[j, 3]) *
						Delta(j, 3 - j) * cDiff +
						diffDelta(j, 3 - j) * cNoDiff;

					c[j] = a - b + cc;
				}
				else if (ef < values[3])
				{
					if (j == 3)
					{
						c[j] =
							diffDelta(0, 3) * Delta(1, 3) * Delta(2, 3) +
							Delta(0, 3) * diffDelta(1, 3) * Delta(2, 3) +
							Delta(0, 3) * Delta(1, 3) * diffDelta(2, 3);
						c[j] *= 4;
					}

					for (int m = 0; m < 3; m++)
					{
						c[j] += (Kronecker[m, j] - Kronecker[j, 3]) *
							DiffDeltaFour(m, 3);
					}
					c[j] = -c[j];
				}
			}
			return c;
		}

		double diffDelta(int j, int jprime)
		{
			return 1.0 / (values[j] - values[jprime]);
		}
		double DiffDeltaFour(int m, int jprime)
		{
			double retval = 4 * Math.Pow(ef - values[jprime], 3);

			retval /= values[m] - values[jprime];
			for (int j = 0; j < 3; j++)
			{
				if (j == jprime) continue;

				retval /= values[j] - values[jprime];
			}

			return retval;
		}
	}
}
