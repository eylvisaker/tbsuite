using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ERY.EMath;

namespace TightBinding
{
	public class TbInputFile : InputReader
	{
		Lattice lattice;
		SiteList sites;
		HoppingPairList hoppings;
		KptList kpath, kmesh;
		KptList qplane;
		List<int> poles = new List<int>();
		double smearing = 0.04;

		public double ChemicalPotential { get; set; }
		public Lattice Lattice { get { return lattice; } }
		public SiteList Sites { get { return sites; } }
		public HoppingPairList Hoppings { get { return hoppings; } }
		public KptList KPath { get { return kpath; } }
		public KptList KMesh { get { return kmesh; } }
		public KptList QPlane { get { return qplane; } }
		public double[] FrequencyMesh { get; private set; }
		public double[] TemperatureMesh { get; private set; }
		public double Smearing { get { return smearing; } }
		public List<int> PoleStates { get { return poles; } }
		public SymmetryList Symmetries { get { return symmetries; } }
		public double Nelec { get; private set; }

		public double HubU, HubUp, HubJ, HubJp;

		int[] kgrid = new int[3];
		int[] shift = new int[] { 1, 1, 1 };
		SymmetryList symmetries = new SymmetryList();

		int[] qgrid = new int[3];
		Vector3[] qplaneDef = new Vector3[3];
		bool setQplane = false;

		public TbInputFile(string filename)
			: base(filename)
		{
		}

		protected override void Validate()
		{
			if (lattice == null)
				ThrowEx(@"""Lattice"" section missing.");
			if (sites == null)
				ThrowEx(@"""Sites"" section missing.");
			if (hoppings == null)
				ThrowEx(@"""Hoppings"" section missing.");
			if (kpath == null)
				kpath = KptList.DefaultPath(lattice);
			if (kgrid == null || kgrid[0] == 0 || kgrid[1] == 0 || kgrid[2] == 0)
				ThrowEx(@"KMesh was not defined properly.");

			if (sites.Count == 0)
				ThrowEx(@"There are no sites.");
			if (hoppings.Count == 0)
				ThrowEx(@"There are no hoppings.");

			foreach (HoppingPair h in hoppings)
			{
				if (h.Left > sites.Count || h.Right > sites.Count)
					ThrowEx(string.Format(@"The hopping {0} to {1} was specified, but there are only {2} sites.",
										  h.Left, h.Right, sites.Count));
			}

		}
		protected override void PostProcess()
		{
			GenerateKmesh();
			
		}

		private void GenerateKmesh()
		{
			kmesh = KptList.GenerateMesh(lattice, kgrid, shift, symmetries, false, false);

			Output.WriteLine("Applied {0} symmetries to get {1} irreducible kpoints from {2}.",
				symmetries.Count, kmesh.Kpts.Count, kmesh.AllKpts.Count);

			if (setQplane)
			{
				qplane = KptList.GeneratePlane(lattice, qplaneDef, symmetries, qgrid);
				Output.WriteLine("Found {0} irreducible qpoints in the plane of {1} qpoints.",
					qplane.Kpts.Count, qplane.AllKpts.Count);
			}
		}

		protected override void ReadSection(string sectionName)
		{
			switch (sectionName)
			{
				case "Lattice":
					ReadLatticeSection();
					break;

				case "Sites":
					ReadSitesSection();
					break;

				case "Hoppings":
					ReadHoppingsSection();
					break;

				case "Hubbard":
					ReadHubbardSection();
					break;

				case "KPath":
					ReadKPathSection("KPath", ref kpath);
					break;

				case "KMesh":
					ReadKMeshSection("KMesh", kgrid);
					break;

				case "QPlane":
					ReadQPlaneSection();
					break;

				case "Poles":
					ReadPolesSection();
					break;

				case "Symmetry":
					ReadSymmetrySection();
					break;

				case "Frequency":
					ReadFrequencySection();
					break;

				case "Temperature":
					ReadTemperatureSection();
					break;

				case "Mu":
					ReadChemicalPotential();
					break;

				case "Nelec":
					ReadNelec();
					break;

				default:
					ThrowEx("Unrecognized section " + sectionName);
					break;
			}
		}

		private void ReadQPlaneSection()
		{
			for (int i = 0; i < 3; i++)
			{
				qgrid[i] = int.Parse(LineWords[i]);
			}

			ReadNextLine();

			for (int i = 0; i < 3; i++)
			{
				qplaneDef[i] = Vector3.Parse(Line);

				ReadNextLine();
			}

			setQplane = true;
		}

		private void ReadHubbardSection()
		{
			HubU = double.Parse(LineWords[0]);
			HubUp = double.Parse(LineWords[1]);
			HubJ = double.Parse(LineWords[2]);
			HubJp = double.Parse(LineWords[3]);
		}


		private void ReadChemicalPotential()
		{
			ChemicalPotential = double.Parse(Line);
		}

		private void ReadNelec()
		{
			Nelec = double.Parse(Line);
		}

		private void ReadFrequencySection()
		{
			FrequencyMesh = ReadDoubleMesh();
		}

		private void ReadTemperatureSection()
		{
			TemperatureMesh = ReadDoubleMesh();
		}

		private double[] ReadDoubleMesh()
		{
			double[] array = new double[1];
			string[] words = LineWords;
			bool singlePoint = false;

			if (words.Length > 3)
			{
				array = new double[words.Length];
				for (int i = 0; i < array.Length; i++)
					array[i] = double.Parse(words[i]);
			}
			else if (words.Length > 1)
			{
				double f1 = double.Parse(words[0]);
				double f2 = double.Parse(words[1]);
				int count = int.Parse(words[2]);

				if (count > 1)
				{
					array = new double[count];
					double delta = (f2 - f1) / (count - 1);

					for (int i = 0; i < count; i++)
					{
						array[i] = f1 + delta * i;
					}
				}
				else
					singlePoint = true;
			}
			else
				singlePoint = true;

			if (singlePoint)
			{
				array = new double[1];
				array[0] = double.Parse(words[0]);
			}
			return array;
		}

		void ReadPolesSection()
		{
			string[] vals = LineWords;

			foreach (string v in vals)
				poles.Add(int.Parse(v) - 1);
		}
		void ReadKPathSection(string section, ref KptList path)
		{
			if (path != null)
				ThrowEx(section + " found twice.");

			Vector3 lastKpt = Vector3.Zero;
			char[] array = new char[] { ' ' };
			const double ptScale = 400;

			path = new KptList();
			while (EOF == false && LineType != LineType.NewSection)
			{
				double dummy;

				string[] vals = Line.Split(array, StringSplitOptions.RemoveEmptyEntries);
				if (vals.Length != 3 && vals.Length != 4)
					ThrowEx("Cannot understand path entry.");

				string text = Line;
				string name = string.Empty;

				if (double.TryParse(vals[0], out dummy) == false)
				{
					text = text.Substring(text.IndexOf(' '));
					name = vals[0];
				}

				Vector3 vecval = Vector3.Parse(text) / 2;
				Vector3 kpt = vecval;
				double length = (kpt - lastKpt).Magnitude;

				path.AddPts(lastKpt, kpt, Math.Max((int)(ptScale * length), 1));
				path.Kpts[path.Kpts.Count - 1].Name = name;

				lastKpt = kpt;

				ReadNextLine();
			}
		}
		void ReadKMeshSection(string section, int[] kgrid)
		{
			char[] array = new char[] { ' ' };
			string[] vals = Line.Split(array, StringSplitOptions.RemoveEmptyEntries);

			if (vals.Length != 3 && vals.Length != 6)
				ThrowEx("Need exactly 3 or 6 integers for kmesh.");
			if (kgrid[0] != 0)
				ThrowEx("Second kmesh found.");

			for (int i = 0; i < 3; i++)
			{
				kgrid[i] = int.Parse(vals[i]);
				if (kgrid[i] == 0)
					ThrowEx("Invalid k-mesh data.");
			}

			if (vals.Length == 6)
			{
				for (int i = 0; i < 3; i++)
					shift[i] = int.Parse(vals[i + 3]);
			}

			ReadNextLine();

			if (LineType == LineType.Numeric)
			{
				smearing = double.Parse(Line);
			}
		}

		void ReadLatticeSection()
		{
			if (lattice != null)
				ThrowEx("Multiple lattice sections found.");

			Vector3[] a = new Vector3[3];

			for (int i = 0; i < 3; i++)
			{
				a[i] = Vector3.Parse(Line);
				ReadNextLine();
			}

			lattice = new Lattice(a[0], a[1], a[2]);

		}
		void ReadSitesSection()
		{
			if (sites != null)
				ThrowEx("Multiple sites sections found.");

			sites = new SiteList();

			while (EOF == false && LineType != LineType.NewSection)
			{
				string[] vals = Line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (vals[0].StartsWith("("))
					vals[0] = vals[0].Substring(1);
				if (vals[2].EndsWith(")"))
					vals[2] = vals[2].Substring(0, vals[2].Length - 1);

				Vector3 loc = new Vector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));

				if (vals.Length > 3)
				{
					string name = vals[3];
					sites.Add(new Site(loc, name));
				}
				else
					sites.Add(new Site(loc));

				ReadNextLine();
			}
		}
		void ReadHoppingsSection()
		{
			if (hoppings != null)
				ThrowEx("Multiple hoppings sections found.");

			if (LineType != LineType.NewSubSection)
				ThrowEx("Hoppings section must start with :..: delimited section.");

			hoppings = new HoppingPairList();

			while (!EOF && LineType != LineType.NewSection)
			{
				string pair = Line.Substring(1, Line.Length - 2);
				string[] values = pair.Split(' ');

				Output.WriteLine(Line);

				if (values.Length != 2)
					ThrowEx("Could not understand hopping pair.");

				int left = int.Parse(values[0]) - 1;
				int right = int.Parse(values[1]) - 1;

				HoppingPair p = new HoppingPair(left, right);
				hoppings.Add(p);

				Output.WriteLine("Reading hoppings for {0}-{1}", left + 1, right + 1);

				ReadNextLine();

				while (LineType == LineType.Hopping || LineType == LineType.Numeric)
				{
					List<string> vals = new List<string>();
					vals.AddRange(Line.Replace('\t', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

					if (vals[0] == "T=") vals.RemoveAt(0);
					if (vals[3] == "hop=") vals.RemoveAt(3);

					double value = double.Parse(vals[vals.Count - 1]);

					Vector3 loc = new Vector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));

					HoppingValue v = new HoppingValue();
					v.Value = value;
					v.R = loc;

					p.Hoppings.Add(v);

					ReadNextLine();
				}

				Output.WriteLine("Count: {0}", p.Hoppings.Count);

			}
		}

		private void ReadSymmetrySection()
		{
			if (symmetries.Count > 0)
				ThrowEx("Second symmetries section encountered.");

			symmetries.Add(Matrix.Identity(3), null);

			while (!EOF && LineType != LineType.NewSection)
			{
				List<string> words = new List<string>();
				List<int> orbitals = new List<int>();
				words.AddRange(Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

				if (words.Count > 1)
				{
					for (int i = 1; i < words.Count; i++)
						orbitals.Add(int.Parse(words[i]) - 1);
				}

				Matrix m = Matrix.Identity(3);
				Matrix n = Matrix.Zero(3);

				StoreEquivalentOrbitals(orbitals);

				switch (words[0].ToLowerInvariant())
				{
					case "e":
						break;
					case "c2(y)":    //1
						m[0, 0] = -1;
						m[2, 2] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "c2(x)":    //2
						m[1, 1] = -1;
						m[2, 2] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "c2(z)":    //3
						m[0, 0] = -1;
						m[1, 1] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "c2(xy)":    //12
						n[0, 1] = 1;
						n[1, 0] = 1;
						n[2, 2] = -1;
						symmetries.Add(n, orbitals);
						break;
					case "c3/4(z)":    //13
						n[0, 1] = 1;
						n[1, 0] = -1;
						n[2, 2] = 1;
						symmetries.Add(n, orbitals);
						break;
					case "c1/4(z)":    //14
						n[0, 1] = -1;
						n[1, 0] = 1;
						n[2, 2] = 1;
						symmetries.Add(n, orbitals);
						break;
					case "c2(x-y)":    //15
						n[0, 1] = -1;
						n[1, 0] = -1;
						n[2, 2] = -1;
						symmetries.Add(n, orbitals);
						break;
					case "i":    //32
						symmetries.Add(-1 * m, orbitals);
						break;
					case "s(y)":    //33
						m[1, 1] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "s(x)":    //34
						m[0, 0] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "s(z)":    //35
						m[2, 2] = -1;
						symmetries.Add(m, orbitals);
						break;
					case "s(xy)":    // 44
						n[0, 1] = -1;
						n[1, 0] = -1;
						n[2, 2] = 1;
						symmetries.Add(n, orbitals);
						break;
					case "s1/4(z)":    //45
						n[0, 1] = -1;
						n[1, 0] = 1;
						n[2, 2] = -1;
						symmetries.Add(n, orbitals);
						break;
					case "s3/4(z)":    //46
						n[0, 1] = 1;
						n[1, 0] = -1;
						n[2, 2] = -1;
						symmetries.Add(n, orbitals);
						break;
					case "s(x-y)":    //47
						n[0, 1] = 1;
						n[1, 0] = 1;
						n[2, 2] = 1;
						symmetries.Add(n, orbitals);
						break;

					default:
						Output.WriteLine("Unrecognized Symmetry {0}.", words[0]);
						ThrowEx("Invalid Symmetry");
						break;
				}

				ReadNextLine();
			}
		}

		private void StoreEquivalentOrbitals(List<int> orbitals)
		{
			for (int i = 0; i < orbitals.Count; i++)
			{
				if (orbitals[i] == i)
					continue;

				if (sites[i].Equivalent.Contains(orbitals[i]) == false)
					sites[i].Equivalent.Add(orbitals[i]);
				if (sites[orbitals[i]].Equivalent.Contains(i) == false)
					sites[orbitals[i]].Equivalent.Add(i);
			}
		}

		private List<int> SwapOrbitals(List<int> orbitalMap, int count)
		{
			int[] retval = new int[orbitalMap.Count];
			for (int i = 0; i < orbitalMap.Count; i++)
				retval[i] = orbitalMap[i];

			SwapOrbitalsImpl(orbitalMap, ref retval, count);

			return retval.ToList();
		}

		private static void SwapOrbitalsImpl(List<int> orbitalMap, ref int[] retval, int count)
		{
			if (count <= 0)
				return;

			int[] newRetval = new int[retval.Length];

			for (int i = 0; i < orbitalMap.Count; i++)
			{
				newRetval[i] = retval[orbitalMap[i]];
			}
			count--;
			retval = newRetval;

			SwapOrbitalsImpl(orbitalMap, ref retval, count);
		}
		IEnumerable<Matrix> GetPossibleSymmetries()
		{
			Matrix m = Matrix.Identity(3);

			// identity
			yield return m;

			// inversion
			yield return -1 * m;

			// reflections
			m[0, 0] = -1;
			yield return m;

			m[0, 0] = 1;
			m[1, 1] = -1;
			yield return m;

			m[1, 1] = 1;
			m[2, 2] = -1;
			yield return m;

			// rotations
			m = Matrix.Zero(3);

			m[1, 0] = 1;
			m[0, 1] = 1;
			m[2, 2] = 1;
			yield return m;

			m = Matrix.Zero(3);

			m[2, 0] = 1;
			m[0, 2] = 1;
			m[1, 1] = 1;
			yield return m;

			m = Matrix.Zero(3);

			m[2, 1] = 1;
			m[1, 2] = 1;
			m[0, 0] = 1;
			yield return m;

			// rotation/reflections
			yield return new Matrix(3, 3,
			                        0, -1, 0,
			                        1, 0, 0,
			                        0, 0, 1);

			yield return new Matrix(3, 3,
			                        0, 1, 0,
			                        -1, 0, 0,
			                        0, 0, 1);

			yield return new Matrix(3, 3,
			                        0, 1, 0,
			                        1, 0, 0,
			                        0, 0, -1);

		}

		void ApplySymmetries()
		{
			List<Matrix> validSyms = new List<Matrix>();
			Matrix reduce = new Matrix(3, 3);
			reduce.SetRows(lattice.G1, lattice.G2, lattice.G3);
			int symIndex = 0;

			using (StreamWriter s = new StreamWriter("syms"))
			{
				foreach (var sym in GetPossibleSymmetries())
				{
					symIndex++;
					s.WriteLine();
					s.WriteLine("Applying symmetry " + symIndex.ToString() + ":");
					s.WriteLine(sym);

					Matrix lat = new Matrix(3, 3);
					lat.SetColumns(sym * lattice.A1, sym * lattice.A2, sym * lattice.A3);

					lat = reduce * lat;

					s.WriteLine("Lattice vector test...");
					if (CheckLatticeSymmetry(lat) == false)
						goto fail;

					Dictionary<int, int> sitemap = new Dictionary<int, int>();

					s.WriteLine("Generating site map...");

					for (int i = 0; i < sites.Count; i++)
					{
						var site = sites[i];
						Vector3 loc = sym * site.Location;

						int index = Sites.FindIndex(lattice, loc);

						if (index == -1)
						{
							s.WriteLine("Failed to map site " + i.ToString());
							goto fail;
						}

						sitemap[i] = index;
						s.WriteLine("  " + i.ToString() + " => " + index.ToString());
					}

					HoppingPairList newHops = new HoppingPairList();

					// rotate hoppings
					for (int i = 0; i < hoppings.Count; i++)
					{
						var pair = hoppings[i];

						int newleft = sitemap[pair.Left];
						int newright = sitemap[pair.Right];

						HoppingPair newPair = new HoppingPair(newleft, newright);
						newHops.Add(newPair);

						foreach (var hop in pair.Hoppings)
						{
							HoppingValue v = new HoppingValue();
							v.Value = hop.Value;
							v.R = sym * hop.R;

							newPair.Hoppings.Add(v);
						}

					}

					s.WriteLine("Performing hopping test...");
					if (newHops.Equals(hoppings) == false)
						goto fail;

					s.WriteLine("Success.");
					validSyms.Add(sym);
					continue;

				fail:
					s.WriteLine("Failed.");
				}

				// now apply symmetries to reduce k-points in kmesh
				int initialKptCount = kmesh.Kpts.Count;

				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();

				foreach (var sym in validSyms)
				{
					for (int i = 0; i < kmesh.Kpts.Count; i++)
					{
						KPoint kpt = kmesh.Kpts[i];
						Vector3 trans = kpt.Value;
						/*
						for (int j = 0; j < 3; j++)
						{
							trans = sym * trans;
							
							int index = kmesh.IndexOf(trans, i+1);
							
							if (index == -1)
								continue;
							
							kmesh.Kpts.RemoveAt(index);
							kpt.Weight ++;
							
						}
						*/
					}
				}
				watch.Stop();

				string fmt = string.Format("{0} total kpts, {1} irreducible kpts.  Applying symmetries took {2} seconds.",
										   initialKptCount, kmesh.Kpts.Count, watch.ElapsedMilliseconds / 1000);

				Output.WriteLine(fmt);
				s.WriteLine(fmt);
			}
		}
		bool CheckLatticeSymmetry(Matrix lat)
		{
			for (int col = 0; col < 3; col++)
			{
				int count = 0;

				for (int row = 0; row < 3; row++)
				{
					double x = lat[row, col].RealPart;

					// skip zeros
					if (Math.Abs(x) < 1e-6)
						continue;

					if (Math.Abs(x) - 1 > 1e-6)
						return false;
					else
						count++;

				}

				if (count != 1)
					return false;
			}

			return true;
		}


		private double GetDist(Vector3 a, Vector3 b)
		{
			return (a - b).MagnitudeSquared;
		}
	}
}