
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBindingSuite
{
	public class OrbitalList : List<Orbital>
	{
		public OrbitalList()
		{
		}
		
		public int FindIndex(Lattice latt, Vector3 pos)
		{
			Vector3 reduced = latt.ReducedCoords(pos, true);
			
			for (int i = 0; i < Count; i++)
			{
				Vector3 tr = latt.ReducedCoords(this[i].Location, true);
				
				if ((reduced - tr).Magnitude < 1e-6)
					return i;
			}
			
			return -1;
		}

		public IEnumerable<int> OrbitalsInInteractionGroup(string group)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].InteractionGroup == group)
					yield return i;
			}
		}
	}

	public class Orbital
	{
		Vector3 vec;
		List<int> equivalentSites = new List<int>();

		public Orbital(Vector3 vec)
		{
			this.vec = vec;
		}
		public Orbital(Vector3 vec, string name, string symmPos, string localSym, string interactionGroup)
		{
			this.vec = vec;
			this.Name = name;
			this.SymmetryPosition = symmPos;
			this.LocalSymmetry = localSym;
			this.InteractionGroup = interactionGroup;
		}

		public string Name { get; private set; }
		public string SymmetryPosition { get; private set; }
		public string LocalSymmetry { get; private set; }
		public string InteractionGroup { get; private set; }
		public Vector3 Location { get { return vec; } set { vec = value; } }
		public List<int> Equivalent { get { return equivalentSites; } }

		public double X { get { return vec.X; } set { vec.X = value; } }
		public double Y { get { return vec.Y; } set { vec.Y = value; } }
		public double Z { get { return vec.Z; } set { vec.Z = value; } }

	}
}
