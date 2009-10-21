
using System;
using System.Collections.Generic;
using ERY.EMath;

namespace TightBindingSuite
{
	public class SiteList : List<Site>
	{
		public SiteList()
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

		public IEnumerable<int> SitesAt(string siteName)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].SiteName == siteName)
					yield return i;
			}
		}
	}

	public class Site
	{
		Vector3 vec;
		string name;
		List<int> equivalentSites = new List<int>();

		public Site(Vector3 vec)
		{
			this.vec = vec;
		}
		public Site(Vector3 vec, string name, string site, string localSym)
		{
			this.vec = vec;
			this.name = name;
			this.SiteName = site;
			this.LocalSymmetry = localSym;
		}

		public string Name { get { return name; } set { name = value; } }
		public string SiteName { get; set; }
		public string LocalSymmetry { get; set; }
		public Vector3 Location { get { return vec; } set { vec = value; } }
		public List<int> Equivalent { get { return equivalentSites; } }

		public double X { get { return vec.X; } set { vec.X = value; } }
		public double Y { get { return vec.Y; } set { vec.Y = value; } }
		public double Z { get { return vec.Z; } set { vec.Z = value; } }

	}
}
