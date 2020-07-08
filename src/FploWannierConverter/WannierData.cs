using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FploWannierConverter
{
	class WannierData
	{
		List<Atom> mAtoms = new List<Atom>();
		List<WannierFunction> mWanniers = new List<WannierFunction>();

		public Grid Grid { get; set; }
		public List<WannierFunction> WannierFunctions { get { return mWanniers; } }
		public List<Atom> Atoms { get { return mAtoms; } }
	}
}
