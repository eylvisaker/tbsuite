using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace FploWannierConverter
{
	class Atom
	{
		/// <summary>
		/// Position of the atom in atomic units (Bohrs)
		/// </summary>
		public Vector3 Position { get; set; }
		public string Element { get; set; }
	}
}
