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
			elements.Add("O", 8);
			elements.Add("Rb", 37);
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
				return elements[Element];
			}
		}
	}
}
