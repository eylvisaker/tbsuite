using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace FploWannierConverter
{
	class Atom
	{
		static Dictionary<string, int> elements = new Dictionary<string, int>();

		static Atom()
		{
			string at = Resources.Atoms;
			string[] lines = at.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

			char[] splitters = new char[] {' ', '\r', '\t', '\n'};
			foreach (var line in lines)
			{
				string[] vals = line.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

				elements.Add(vals[1], int.Parse(vals[0]));
			}
		}

		/// <summary>
		/// Position of the atom in atomic units (Bohrs)
		/// </summary>
		public Vector3 Position { get; set; }
		public string Element { get; set; }

		public int AtomicNumber
		{
			get
			{
				if (string.IsNullOrEmpty(Element))
					return 0;

				return elements[Element];
			}
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", Element, Position.ToString("0.000"));
		}
	}
}
