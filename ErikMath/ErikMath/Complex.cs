using System;
using System.Collections.Generic;
using System.Text;


namespace ERY.EMath
{
	[Serializable]
	public struct Complex
	{
		public const double TOLERANCE = 1e-10;

		private double _x, _y;

		[Obsolete("Use RealPart instead.")]
		public double mx { get { return _x; } set { _x = value; } }
		[Obsolete("Use ImagPart instead.")]
		public double my { get { return _y; } set { _y = value; } }

		public double RealPart { get { return _x; } set { _x = value; } }
		public double ImagPart { get { return _y; } set { _y = value; } }

		public Complex(double real)
		{
			_x = real;
			_y = 0;
		}
		public Complex(double real, double imag)
		{
			_x = real;
			_y = imag;
		}
		public static implicit operator Complex(double x)
		{
			return new Complex(x);
		}

		public void Set(double real, double imag)
		{
			_x = real;
			_y = imag;
		}

		public void SetPolar(double mag, double arg)
		{
			_x = mag * Math.Cos(arg);
			_y = mag * Math.Sin(arg);
		}



		public override string ToString()
		{
			return ToString("");
		}

		public string ToString(string formatString)
		{
			string buffer;
			double rx = _x, ry = _y;

			if (formatString != "")
				formatString = ":" + formatString;

			string real = string.Format("{0" + formatString + "}", _x);
			string imag = string.Format("{0" + formatString + "}", Math.Abs(_y));

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
				return _x * _x + _y * _y;
			}
		}
		public double Argument
		{
			get
			{
				return Math.Atan2(_y, _x);
			}
		}

		public Complex Conjugate()
		{
			return new Complex(_x, -_y);
		}
		public Complex Invert()
		{
			double denom = _x * _x + _y * _y;

			Complex retval = new Complex(_x / denom, -_y / denom);

			return retval;
		}

		public static Complex operator -(Complex t)
		{
			return new Complex(-t._x, -t._y);
		}
		public static Complex operator +(Complex a, Complex b)
		{
			return new Complex(a._x + b._x, a._y + b._y);
		}
		public static Complex operator -(Complex a, Complex b)
		{
			return new Complex(a._x - b._x, a._y - b._y);
		}
		public static Complex operator *(Complex a, Complex b)
		{
			double new_x = a._x * b._x - a._y * b._y;
			double new_y = a._x * b._y + a._y * b._x;

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
			return new Complex(t._x * r, t._y * r);
		}
		public static Complex operator /(double r, Complex t)
		{
			return new Complex(r) / t;
		}

		public static Complex operator *(Complex t, double r)
		{
			return new Complex(t._x * r, t._y * r);
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

			retval.SetPolar(Math.Exp(t._x), t._y);

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
				Math.Pow(t.Magnitude, u._x) * Math.Exp(-t.Argument * u._y),
					t.Argument * u._x + u._y * Math.Log(t.Magnitude));

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
			return new Complex(Math.Sin(t._x) * Math.Cosh(t._y), Math.Cos(t._x) * Math.Sinh(t._y));
		}
		public static Complex Cos(Complex t)
		{
			return new Complex(Math.Cos(t._x) * Math.Cosh(t._y), -Math.Sin(t._x) * Math.Sinh(t._y));
		}
		public static Complex Tan(Complex t)
		{
			return new Complex(Math.Tan(t._x), Math.Tanh(t._y)) /
				new Complex(1, -Math.Tan(t._x) * Math.Tanh(t._y));
		}

		public static bool operator ==(Complex a, Complex b)
		{
			double dx = a._x - b._x;
			double dy = a._y - b._y;

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
			Complex retval = new Complex(_x, _y);

			if (Math.Abs(_x) < tolerance) retval._x = 0;
			if (Math.Abs(_y) < tolerance) retval._y = 0;

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
			return _x.GetHashCode() - _y.GetHashCode();
		}

		public static Complex Parse(string real)
		{
			return new Complex(double.Parse(real));
		}
		public static Complex Parse(string real, string imag)
		{
			return new Complex(double.Parse(real), double.Parse(imag));
		}
	}
}