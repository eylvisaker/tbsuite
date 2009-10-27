using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace ERY.EMath
{
	[Serializable]
	public struct Complex
	{
		public const double TOLERANCE = 1e-10;

		public double x, y;

		public double RealPart { get { return x; } set { x = value; } }
		public double ImagPart { get { return y; } set { y = value; } }

		public Complex(double real)
		{
			x = real;
			y = 0;
		}
		public Complex(double real, double imag)
		{
			x = real;
			y = imag;
		}
		public static implicit operator Complex(double x)
		{
			return new Complex(x);
		}

		public void Set(double real, double imag)
		{
			x = real;
			y = imag;
		}

		public void SetPolar(double mag, double arg)
		{
			x = mag * Math.Cos(arg);
			y = mag * Math.Sin(arg);
		}



		public override string ToString()
		{
			return ToString("");
		}

		public string ToString(string formatString)
		{
			string buffer;
			double rx = x, ry = y;

			if (formatString != "")
				formatString = ":" + formatString;

			string real = string.Format("{0" + formatString + "}", x);
			string imag = string.Format("{0" + formatString + "}", Math.Abs(y));

			if (double.Parse(imag) == 0)
				ry = 0;

			if ((ry > 0 && imag != "") && (rx != 0 && real != ""))
				buffer = "(" + real + " + " + imag + "i)";
			else if ((ry < 0 && imag != "") && (rx != 0 && real != ""))
				buffer = "(" + real + " - " + imag + "i)";
			else if (ry > 0 && imag != "")
				buffer = imag + "i";
			else if (ry < 0 && imag != "")
				buffer = "-" + imag + "i";
			else
				buffer = real;


			return buffer;
		}

		public double Magnitude
		{
			get
			{
				return Math.Sqrt(MagnitudeSquared);
			}
		}
		public double MagnitudeSquared
		{
			get
			{
				return x * x + y * y;
			}
		}
		public double Argument
		{
			get
			{
				return Math.Atan2(y, x);
			}
		}

		public Complex Conjugate()
		{
			return new Complex(x, -y);
		}
		public Complex Invert()
		{
			double denom = x * x + y * y;

			Complex retval = new Complex(x / denom, -y / denom);

			return retval;
		}

		public static Complex operator -(Complex t)
		{
			return new Complex(-t.x, -t.y);
		}
		public static Complex operator +(Complex a, Complex b)
		{
			return new Complex(a.x + b.x, a.y + b.y);
		}
		public static Complex operator -(Complex a, Complex b)
		{
			return new Complex(a.x - b.x, a.y - b.y);
		}
		public static Complex operator *(Complex a, Complex b)
		{
			double new_x = a.x * b.x - a.y * b.y;
			double new_y = a.x * b.y + a.y * b.x;

			return new Complex(new_x, new_y);
		}
		public static Complex operator /(Complex a, Complex b)
		{
			return a * b.Invert();
		}
		// Possibly optimized version?
		//public static Complex operator /(Complex lhs, Complex rhs)
		//{
		//    Complex result = new Complex();
		//    double e;
		//    double f;
		//    if (System.Math.Abs(rhs.ImagPart) < System.Math.Abs(rhs.RealPart))
		//    {
		//        e = rhs.ImagPart / rhs.RealPart;
		//        f = rhs.RealPart + rhs.ImagPart * e;
		//        result.RealPart = (lhs.RealPart + lhs.ImagPart * e) / f;
		//        result.ImagPart = (lhs.ImagPart - lhs.RealPart * e) / f;
		//    }
		//    else
		//    {
		//        e = rhs.RealPart / rhs.ImagPart;
		//        f = rhs.ImagPart + rhs.RealPart * e;
		//        result.RealPart = (lhs.ImagPart + lhs.RealPart * e) / f;
		//        result.ImagPart = (-lhs.RealPart + lhs.ImagPart * e) / f;
		//    }
		//    return result;
		//}
		public static Complex operator /(Complex a, double b)
		{
			return new Complex(a.RealPart / b, a.ImagPart / b);
		}
		public static Complex operator +(double r, Complex t)
		{
			return new Complex(t.RealPart + r, t.ImagPart);
		}
		public static Complex operator +(Complex t, double r)
		{
			return new Complex(t.RealPart + r, t.ImagPart);
		}
		public static Complex operator -(double r, Complex t)
		{
			return new Complex(r - t.RealPart, -t.ImagPart);
		}
		public static Complex operator -(Complex t, double r)
		{
			return new Complex(t.RealPart - r, t.ImagPart);
		}
		public static Complex operator *(double r, Complex t)
		{
			return new Complex(t.x * r, t.y * r);
		}
		public static Complex operator /(double r, Complex t)
		{
			return new Complex(r) / t;
		}

		public static Complex operator *(Complex t, double r)
		{
			return new Complex(t.x * r, t.y * r);
		}

		public static Complex Hypot(Complex a, Complex b)
		{
			Complex r;
			if (a.Magnitude > b.Magnitude)
			{
				r = b / a;
				r = a.Magnitude * Complex.Sqrt(1 + r * r);
			}
			else if (b != 0)
			{
				r = a / b;
				r = b.Magnitude * Complex.Sqrt(1 + r * r);
			}
			else
			{
				r = 0.0;
			}
			return r;
		}
		public static Complex Exp(Complex t)
		{
			Complex retval = new Complex();

			retval.SetPolar(Math.Exp(t.x), t.y);

			return retval;
		}
		public static Complex Log(Complex t)
		{
			Complex retval = new Complex();

			retval.Set(Math.Log(t.Magnitude), t.Argument);

			return retval;
		}
		public static Complex Pow(Complex t, Complex u)
		{
			if (t == 0 && u != 0)
				return 0;

			Complex retval = new Complex();


			retval.SetPolar(
				Math.Pow(t.Magnitude, u.x) * Math.Exp(-t.Argument * u.y),
					t.Argument * u.x + u.y * Math.Log(t.Magnitude));

			return retval;
		}
		public static Complex Sqrt(Complex t)
		{
			Complex retval = new Complex();

			retval.SetPolar(Math.Sqrt(t.Magnitude), t.Argument / 2);

			return retval;
		}
		public static Complex Sin(Complex t)
		{
			return new Complex(Math.Sin(t.x) * Math.Cosh(t.y), Math.Cos(t.x) * Math.Sinh(t.y));
		}
		public static Complex Cos(Complex t)
		{
			return new Complex(Math.Cos(t.x) * Math.Cosh(t.y), -Math.Sin(t.x) * Math.Sinh(t.y));
		}
		public static Complex Tan(Complex t)
		{
			return new Complex(Math.Tan(t.x), Math.Tanh(t.y)) /
				new Complex(1, -Math.Tan(t.x) * Math.Tanh(t.y));
		}

		public static bool operator ==(Complex a, Complex b)
		{
			double dx = a.x - b.x;
			double dy = a.y - b.y;

			if (dx < 0) dx = -dx;
			if (dy < 0) dy = -dy;

			if (dx > TOLERANCE)
				return false;
			if (dy > TOLERANCE)
				return false;
			else
				return true;
		}
		public static bool operator !=(Complex a, Complex b)
		{
			return !(a == b);
		}

		public Complex Round()
		{
			return Round(TOLERANCE);
		}
		public Complex Round(double tolerance)
		{
			Complex retval = new Complex(x, y);

			if (Math.Abs(x) < tolerance) retval.x = 0;
			if (Math.Abs(y) < tolerance) retval.y = 0;

			return retval;
		}

		public override bool Equals(object obj)
		{
			if (obj is Complex)
				return this == (Complex)obj;
			else if (obj is double)
			{
				return this == new Complex((double)obj);
			}
			else
				return false;
		}
		public override int GetHashCode()
		{
			return x.GetHashCode() - y.GetHashCode();
		}

		public static Complex Parse(string val)
		{
			val = val.Trim();

			Regex number = new Regex(@"[+-]? *[0-9]+(\.[0-9]+)?([dDeE][+-][0-9]*)?[^i]");
			Regex cplxr = new Regex(@"[+-]?[0-9]+(\.[0-9]+)?([dDeE][+-][0-9]*)? *[+-] *[0-9]+(\.[0-9]+)?([dDeE][+-][0-9]*)?i");
			Regex imaginary = new Regex(@"[+-]? *[0-9]+(\.[0-9]+)?([dDeE][+-][0-9]*)?i");

			var m = cplxr.Matches(val, 0);
			var im = imaginary.Matches(val, 0);

			if (m.Count == 0 && im.Count == 0)
				return new Complex(double.Parse(val));
			else if (m.Count == 0)
			{
				string imag = val.Remove(val.IndexOf('i'));

				Complex retval = new Complex(0, double.Parse(imag));
				return retval;
			}
			else
			{
				m = number.Matches(val, 0);

				string real = m[0].ToString().Trim();
				string imag = m[1].ToString().Replace(" ", "");

				Complex retval = new Complex(double.Parse(real), double.Parse(imag));
				return retval;
			}
		}
		public static Complex Parse(string real, string imag)
		{
			return new Complex(double.Parse(real), double.Parse(imag));
		}
	}
}