using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath
{
	public class Pair<T1,T2>
	{
		public Pair(T1 a, T2 b)
		{
			First = a;
			Second = b;
		}

		public T1 First { get; set; }
		public T2 Second { get; set; }

		public override string ToString()
		{
			return string.Format(
				"{{}{0}, {1}{}}", First, Second);

		}
	}
	public class Triplet<T1, T2, T3>
	{
		public Triplet(T1 a, T2 b, T3 c)
		{
			First = a;
			Second = b;
			Third = c;
		}

		public T1 First { get; set; }
		public T2 Second { get; set; }
		public T3 Third { get; set; }
	}
}
