using System;
using ERY.EMath;


namespace AlgLib.AP
{
	/********************************************************************
	AP math namespace
	********************************************************************/
	public struct rcommstate
	{
		public int stage;
		public int[] ia;
		public bool[] ba;
		public double[] ra;
		public Complex[] ca;
	};

	/********************************************************************
    AP math namespace
    ********************************************************************/

	public class APMath
	{
		public static System.Random RndObject = new System.Random(System.DateTime.Now.Millisecond);

		public const double MachineEpsilon = 5E-16;
		public const double MaxRealNumber = 1E300;
		public const double MinRealNumber = 1E-300;

		public static double RandomReal()
		{
			double r = 0;
			lock (RndObject) { r = RndObject.NextDouble(); }
			return r;
		}
		public static int RandomInteger(int N)
		{
			int r = 0;
			lock (RndObject) { r = RndObject.Next(N); }
			return r;
		}
		public static double Sqr(double X)
		{
			return X * X;
		}
		public static double AbsComplex(Complex z)
		{
			double w;
			double xabs;
			double yabs;
			double v;

			xabs = System.Math.Abs(z.RealPart);
			yabs = System.Math.Abs(z.ImagPart);
			w = xabs > yabs ? xabs : yabs;
			v = xabs < yabs ? xabs : yabs;
			if (v == 0)
				return w;
			else
			{
				double t = v / w;
				return w * System.Math.Sqrt(1 + t * t);
			}
		}
		public static Complex Conj(Complex z)
		{
			return z.Conjugate();
		}
		public static Complex CSqr(Complex z)
		{
			return z * z;
		}

	}
}