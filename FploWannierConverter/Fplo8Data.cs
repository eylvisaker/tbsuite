using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ERY.EMath;

namespace FploWannierConverter
{
	class Fplo8Data
	{
		Vector3[] lattice = new Vector3[3];
		Vector3[] reciprocal = new Vector3[3];

		List<Atom> sites = new List<Atom>();

		public void FindAtoms(WannierData retval)
		{
			const int size = 4;

			Vector3 origin = retval.Grid.Origin;
			Vector3[] span = retval.Grid.SpanVectors;

			retval.Atoms.Clear();

			foreach (Atom atom in sites)
			{
				for (int k = -size; k <= size; k++)
				{
					for (int j = -size; j <= size; j++)
					{
						for (int i = -size; i <= size; i++)
						{
							Vector3 v = i * lattice[0] + j * lattice[1] + k * lattice[2];
							Vector3 loc = atom.Position + v;

							Vector3 diffLoc = loc - origin;
							bool bad = false;

							for (int l = 0; l < 3; l++)
							{
								double dot = span[l].DotProduct(diffLoc);
								double dist = dot / span[l].Magnitude;

								if (dot < 0) bad = true;
								if (dist > span[l].Magnitude) bad = true;
							}

							if (bad)
								continue;

							Atom a = new Atom();
							a.Element = atom.Element;
							a.Position = loc;

							retval.Atoms.Add(a);
						}
					}
				}
			}
		}
		internal void oldFindAtoms(WannierData retval)
		{
			for (int i = 0; i < 3; i++)
			{
				if (lattice[i].Magnitude == 0)
					throw new InvalidOperationException("Lattice vectors have not been read.");
			}
			if (sites.Count == 0)
				throw new InvalidOperationException("Atom sites have not been read.");

			for (int i = 0; i < retval.Atoms.Count; i++)
			{
				Atom a = retval.Atoms[i];
				Vector3 red = ReduceRealSpace(a.Position);

				double minDist = double.MaxValue;
				Atom minSite = null;

				for (int j = 0; j < sites.Count; j++)
				{
					Vector3 site_red = ReduceRealSpace(sites[j].Position);

					Vector3 diff = red - site_red;

					if (diff.Magnitude < minDist)
					{
						minDist = diff.Magnitude;
						minSite = sites[j];
					}
				}

				if (minDist > 1e-6)
					throw new Exception(string.Format("Could not find atom {0}.", i));

				a.Element = minSite.Element;
			}
		}

		private Vector3 ReduceRealSpace(Vector3 vector3)
		{
			Vector3 retval = new Vector3(
				reciprocal[0].DotProduct(vector3),
				reciprocal[1].DotProduct(vector3),
				reciprocal[2].DotProduct(vector3));

			for (int i = 0; i < 3; i++)
			{
				while (retval[i] < 0) retval[i] += 1;
				while (retval[i] >= 1) retval[i] -= 1;
			}

			return retval;
		}

		internal void ReadLatticeVectors(System.IO.StreamReader reader, Fplo8Data data)
		{
			for (int i = 0; i < 3; i++)
			{
				string line = reader.ReadLine().Trim();

				if (line.StartsWith(string.Format("a{0}", i + 1)) == false)
					throw new InvalidDataException("Could not read lattice vectors.");

				string[] text = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				int len = text.Length;
				lattice[i] = Vector3.Parse(text[len - 3], text[len - 2], text[len - 1]);
			}

			Matrix A = new Matrix(3, 3,
				lattice[0].X, lattice[0].Y, lattice[0].Z,
				lattice[1].X, lattice[1].Y, lattice[1].Z,
				lattice[2].X, lattice[2].Y, lattice[2].Z);

			Matrix G = A.Invert().Transpose();

			for (int i = 0; i < 3; i++)
				reciprocal[i] = new Vector3(G[i, 0].x, G[i, 1].x, G[i, 2].x);
		}

		internal void ReadAtomSites(System.IO.StreamReader reader, Fplo8Data data, int count)
		{
			if (count == 0)
				throw new ArgumentException("Number of sites can't be zero.");

			for (int i = 0; i < count; i++)
			{
				string line = reader.ReadLine().Trim();

				string[] text = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				Atom a = new Atom();
				a.Element = text[1];

				int len = text.Length;
				a.Position = Vector3.Parse(text[len - 3], text[len - 2], text[len - 1]);

				sites.Add(a);
			}
		}

	}
}
