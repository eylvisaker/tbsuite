using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath
{
    [Serializable]
    public struct Vector3
    {
        private double mx, my, mz;

        public const double TOLERANCE = 1e-10;

        public Vector3(Vector3 r) { mx = r.X; my = r.Y; mz = r.Z; }
        public Vector3(double x, double y, double z) { mx = x; my = y; mz = z; }

        public void set(double x, double y, double z) { mx = x; my = y; mz = z; }

        public static readonly Vector3 Zero = new Vector3(0,0,0);
		
        public double this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0: return mx;
                    case 1: return my;
                    case 2: return mz;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: mx = value; break;
                    case 1: my = value; break;
                    case 2: mz = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public double X
        {
            get { return mx; }
            set { mx = value; }
        }
        public double Y
        {
            get { return my; }
            set { my = value; }
        }
        public double Z
        {
            get { return mz; }
            set { mz = value; }
        }

        public double Theta
        {
            get { return Math.Acos(Z / Magnitude); }
        }
        public double Phi
        {
            get { return Math.Atan2(Y, X); }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }
        public double MagnitudeSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public double DotProduct(Vector3 r)
        {
            return X * r.X + Y * r.Y + Z * r.Z;
        }
        public Vector3 CrossProduct(Vector3 r)
        {
            return new Vector3(this[1] * r[2] - this[2] * r[1],
                              this[2] * r[0] - this[0] * r[2],
                              this[0] * r[1] - this[1] * r[0]);
        }

		public static Vector3 CrossProduct(Vector3 left, Vector3 right)
		{
			return new Vector3(left[1] * right[2] - left[2] * right[1],
							   left[2] * right[0] - left[0] * right[2],
							   left[0] * right[1] - left[1] * right[0]);
		}

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            Vector3 retval = new Vector3();

            for (int i = 0; i < 3; i++)
                retval[i] = a[i] + b[i];

            return retval;
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            Vector3 retval = new Vector3();

            for (int i = 0; i < 3; i++)
                retval[i] = a[i] - b[i];

            return retval;
        }
        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a[0], -a[1], -a[2]);
        }
        public static Vector3 operator *(Vector3 a, double r)
        {
            Vector3 retval = new Vector3();

            for (int i = 0; i < 3; i++)
                retval[i] = a[i] * r;

            return retval;
        }
        public static Vector3 operator /(Vector3 a, double r)
        {
            Vector3 retval = new Vector3();

            for (int i = 0; i < 3; i++)
                retval[i] = a[i] / r;

            return retval;
        }
        /*
            public static explicit operator Vector3<G> (Vector3 double)  
            {
                return Vector3<G>(double.x, double.y, double.z);
            }
        */

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            Vector3 t = a - b;

            if (t.Magnitude <= TOLERANCE * a.Magnitude)
                return true;
            else
                return false;
        }
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }
        

        public static Vector3 operator *(double r1, Vector3 r2)
        {
            return r2 * r1;
        }

        public override string ToString()
        {
            string buffer;
            buffer = string.Format("({0}, {1}, {2})", (double)this[0], (double)this[1], (double)this[2]);
            return buffer;
        }
		public string ToString(string format)
		{
			string buffer;
			buffer = string.Format("({0:" + format + "}, {1:" + format + "}, {2:" + format + "})", 
				(double)this[0], (double)this[1], (double)this[2]);

			return buffer;
		}

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                return this == (Vector3)obj;
            }
            else
                return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public static Vector3 Parse(string text)
		{
			Vector3 retval;
			if (TryParse(text, out retval) == false)
				throw new InvalidCastException(string.Format(
				    @"Could not parse text ""{0}""", text));
			return retval;
		}
		
		public static bool TryParse(string text, out Vector3 value)
		{
			if (text.StartsWith("(") && text.EndsWith(")"))
				text = text.Substring(1, text.Length - 2);
			
			string[] vals = text.Split(new char[]{ ' ',',','\t' }, StringSplitOptions.RemoveEmptyEntries);
						
			value = new Vector3();
			
			if (vals.Length < 3)
				return false;
			
			
			bool valid = true;
			
			valid &= double.TryParse(vals[0], out value.mx);
			valid &= double.TryParse(vals[1], out value.my);
			valid &= double.TryParse(vals[2], out value.mz);
			
			return valid;
		}


		public static Vector3 Parse(string x, string y, string z)
		{
			return new Vector3(
				double.Parse(x),
				double.Parse(y),
				double.Parse(z));
		}
		
		public static Vector3 SphericalPolar(double r, double theta, double phi)
		{
			return new Vector3(r * Math.Sin(theta) * Math.Cos(phi),
			                   r * Math.Sin(theta) * Math.Sin(phi),
			                   r * Math.Cos(theta));
		}
	}
}