using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ERY.EMath;

namespace TightBindingSuite
{
	public partial class TightBinding
	{
		public class TbInputFileReader : InputReader
		{
			TightBinding tb;

			public TbInputFileReader(string filename, TightBinding tb)
				: base(filename)
			{
				this.tb = tb;
			}

			protected override void Validate()
			{
				if (tb.lattice == null)
					ThrowEx(@"""Lattice"" section missing.");
				if (tb.sites == null)
					ThrowEx(@"""Sites"" section missing.");
				if (tb.hoppings == null)
					ThrowEx(@"""Hoppings"" section missing.");
				if (tb.kpath == null)
					tb.kpath = KptList.DefaultPath(tb.lattice);
				if (tb.kgrid == null || tb.kgrid[0] == 0 || tb.kgrid[1] == 0 || tb.kgrid[2] == 0)
					ThrowEx(@"KMesh was not defined properly.");

				if (tb.sites.Count == 0)
					ThrowEx(@"There are no sites.");
				if (tb.hoppings.Count == 0)
					ThrowEx(@"There are no hoppings.");
				if (tb.symmetries.Count == 0)
					tb.symmetries.Add(new Symmetry(Matrix.Identity(3)));

				if (tb.Nelec != null)
				{
					if (tb.MuMesh != null)
						ThrowEx(@"Specify only one of Nelec or Mu.");

					tb.MuMesh = new double[tb.Nelec.Length];

					for (int i = 0; i < tb.Nelec.Length; i++)
					{
						if (tb.Nelec[i] > tb.Orbitals.Count * 2)
							ThrowEx(@"Nelec is too large.");
						else if (tb.Nelec[i] < 0)
							ThrowEx(@"Nelec cannot be less than zero.");
					}
				}
				
				if (tb.MuMesh == null)
					tb.MuMesh = new double[] { 0 };
				if (tb.TemperatureMesh == null)
					tb.TemperatureMesh = new double[] { 1 };
				if (tb.FrequencyMesh == null)
					tb.FrequencyMesh = new double[] { 0 };

				foreach (HoppingPair h in tb.hoppings)
				{
					if (h.Left >= tb.sites.Count || h.Right >= tb.sites.Count)
						ThrowEx(string.Format(@"The hopping {0} to {1} was specified, but there are only {2} sites.",
											  h.Left+1, h.Right+1, tb.sites.Count));
				}

				foreach (Orbital orb in tb.Orbitals)
				{
					var interactionOrbs = tb.Orbitals.Where((x,y) => x.InteractionGroup == orb.InteractionGroup);

					foreach (Orbital otherOrb in interactionOrbs)
					{
						Vector3 delta = otherOrb.Location - orb.Location;

						if (delta.Magnitude > 1e-8)
						{
							ThrowEx(string.Format("In the interaction group {0}, orbitals {1} and {2} are present, but they are in different positions.",
								orb.InteractionGroup, orb.Name, otherOrb.Name));
						}
					}
				}
			}
			protected override void PostProcess()
			{
				GenerateKmesh();

			}

			private void GenerateKmesh()
			{
				tb.kmesh = KptList.GenerateMesh(tb.lattice, tb.kgrid, tb.shift, tb.symmetries, false);

				Output.WriteLine("Applied {0} symmetries to get {1} irreducible kpoints from {2}.",
					tb.symmetries.Count, tb.kmesh.Kpts.Count, tb.kmesh.AllKpts.Count);

				using (StreamWriter writer = new StreamWriter("kpts"))
				{
					for (int i = 0; i < tb.kmesh.Kpts.Count; i++)
					{
						Vector3 red = tb.lattice.ReducedCoords(tb.kmesh.Kpts[i].Value);

						writer.WriteLine("{0}     {1}", i, red);
					}
				}

				if (tb.setQplane)
				{
					tb.qplane = KptList.GeneratePlane(tb.lattice, tb.qplaneDef, tb.symmetries, tb.qgrid, null);
					Output.WriteLine("Found {0} irreducible qpoints in the plane of {1} qpoints.",
						tb.qplane.Kpts.Count, tb.qplane.AllKpts.Count);


					using (StreamWriter writer = new StreamWriter("qpts"))
					{
						for (int i = 0; i < tb.qplane.Kpts.Count; i++)
						{
							Vector3 red = tb.lattice.ReducedCoords(tb.qplane.Kpts[i].Value);

							writer.WriteLine("{0}     {1}", i, red);
						}
					}
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
						ThrowEx("Sites section has been renamed to \"Orbitals\".");
						break;

					case "Orbitals":
						ReadOrbitalsSection();
						break;

					case "Hoppings":
						ReadHoppingsSection();
						break;

					case "Hubbard":
						ThrowEx("Hubbard section is obsolete.  Please use the interaction section.");
						break;

					case "Interaction":
						ReadInteractionSection();
						break;

					case "KPath":
						ReadKPathSection("KPath", ref tb.kpath);
						break;

					case "KMesh":
						ReadKMeshSection("KMesh", tb.kgrid);
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
					tb.qgrid[i] = int.Parse(LineWords[i]);
				}

				ReadNextLine();

				for (int i = 0; i < 3; i++)
				{
					tb.qplaneDef[i] = Vector3.Parse(Line);

					ReadNextLine();
				}

				tb.setQplane = true;
			}



			private void ReadNelec()
			{
				tb.Nelec = ReadDoubleMesh();
				tb.specifiedNelec = true;
			}
			private void ReadChemicalPotential()
			{
				tb.MuMesh = ReadDoubleMesh();
			}
			private void ReadFrequencySection()
			{
				tb.FrequencyMesh = ReadDoubleMesh();
			}

			private void ReadTemperatureSection()
			{
				tb.TemperatureMesh = ReadDoubleMesh();
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
				else if (words.Length == 3)
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
				else if (words.Length == 2)
				{
					array = new double[2];
					array[0] = double.Parse(words[0]);
					array[1] = double.Parse(words[1]);
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
					tb.poles.Add(int.Parse(v) - 1);
			}
			void ReadKPathSection(string section, ref KptList path)
			{
				if (path != null)
					ThrowEx(section + " found twice.");

				Vector3 lastKpt = Vector3.Zero;
				char[] array = new char[] { ' ' };
				const double ptScale = 400;
				int ptCount = 0;

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

					Vector3 vecval = Vector3.Parse(text);
					Vector3 kpt = vecval;
					double length = (kpt - lastKpt).Magnitude;

					if (ptCount == 0)
					{
						path.AddPts(kpt, kpt, 1);
					}
					else
					{
						path.AddPts(lastKpt, kpt, Math.Max((int)(ptScale * length), 1));
					}
					path.Kpts[path.Kpts.Count - 1].Name = name;
					ptCount++;

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
					tb.kgrid[i] = int.Parse(vals[i]);
					if (tb.kgrid[i] == 0)
						ThrowEx("Invalid k-mesh data.");
				}

				if (vals.Length == 6)
				{
					for (int i = 0; i < 3; i++)
						tb.shift[i] = int.Parse(vals[i + 3]);
				}

				ReadNextLine();
			}

			void ReadLatticeSection()
			{
				if (tb.lattice != null)
					ThrowEx("Multiple lattice sections found.");

				Vector3[] a = new Vector3[3];

				for (int i = 0; i < 3; i++)
				{
					a[i] = Vector3.Parse(Line);
					ReadNextLine();
				}

				tb.lattice = new Lattice(a[0], a[1], a[2]);

			}
			void ReadOrbitalsSection()
			{
				if (tb.sites != null)
					ThrowEx("Multiple sites sections found.");

				tb.sites = new OrbitalList();

				while (EOF == false && LineType != LineType.NewSection)
				{
					string[] vals = Line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					if (vals[0].StartsWith("("))
						vals[0] = vals[0].Substring(1);
					if (vals[2].EndsWith(")"))
						vals[2] = vals[2].Substring(0, vals[2].Length - 1);

					if (vals.Length < 7)
						ThrowEx("Insufficient information about site.");

					Vector3 loc = new Vector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));
					string name = vals[3];
					string sitename = vals[4];
					string localsym = vals[5];
					string interactionGroup = vals[6];

					tb.sites.Add(new Orbital(loc, name, sitename, localsym, interactionGroup));

					ReadNextLine();
				}
			}
			void ReadHoppingsSection()
			{
				if (tb.hoppings != null)
					ThrowEx("Multiple hoppings sections found.");

				if (LineType != LineType.NewSubSection)
					ThrowEx("Hoppings section must start with :..: delimited section.");

				tb.hoppings = new HoppingPairList();

				while (!EOF && LineType != LineType.NewSection)
				{
					string[] values = ReadSubSectionParameters();

					int left = int.Parse(values[0]) - 1;
					int right = int.Parse(values[1]) - 1;

					HoppingPair p = new HoppingPair(left, right);
					tb.hoppings.Add(p);

					//Output.WriteLine("Reading hoppings for {0}-{1}", left + 1, right + 1);

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

					//Output.WriteLine("Count: {0}", p.Hoppings.Count);

				}
			}

			private void ReadInteractionSection()
			{
				if (tb.Interactions != null)
					ThrowEx("Multiple Interaction sections found.");

				tb.Interactions = new InteractionList();

				if (LineType != LineType.NewSubSection && LineType != LineType.NewSection)
				{
					if (Line.ToLowerInvariant().StartsWith( "adjust"))
					{
						tb.Interactions.AdjustInteractions = true;

						string ltext = Line.ToLowerInvariant().Substring(6);
						double val;

						if (double.TryParse(ltext, out val) == false)
						{
							val = 0.001;
							
						}

						if (val <= 0 || val >= 1)
						{
							ThrowEx("Interaction adjustment should be between 0 and 1.");
						}

						tb.Interactions.MaxEigenvalue = 1 - val;
					}

					ReadNextLine();
				}


				while (!EOF && LineType != LineType.NewSection)
				{
					if (LineType != LineType.NewSubSection)
						ThrowEx("Could not understand contents of interaction section.");

					string[] values = ReadSubSectionParameters();

					InteractionPair inter = new InteractionPair(tb.sites, values[0], values[1]);

					if (inter.OrbitalsLeft.Count == 0) ThrowEx("Could not identify interaction group \"" + values[0] + "\".");
					if (inter.OrbitalsRight.Count == 0) ThrowEx("Could not identify interaction group \"" + values[1] + "\".");

					ReadNextLine();
					string[] interactionVals = Line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					double[] vals = interactionVals.Select(x => double.Parse(x)).ToArray();

					ReadNextLine();

					while (!EOF && LineType != LineType.NewSection && LineType != LineType.NewSubSection)
					{
						Vector3 vec = Vector3.Parse(Line);

						inter.Vectors.Add(vec);

						ReadNextLine();
					}

					Orbital left = tb.Orbitals[inter.OrbitalsLeft[0]];
					Orbital right = tb.Orbitals[inter.OrbitalsRight[0]];

					Vector3 delta = left.Location - right.Location;
					if (delta.Magnitude > 1e-8 && inter.Vectors.Count == 0)
					{
						ThrowEx(string.Format(
							"Interaction is specified between groups {0} and {1}, but they are at different locations and no vectors were given!",
							values[0], values[1]));
					}

					if (inter.OnSite)
					{
						if (interactionVals.Length > 4)
							ThrowEx("Found too many parameters in the interaction.");

						inter.HubbardU = double.Parse(interactionVals[0]);

						if (interactionVals.Length > 1) inter.InterorbitalU = double.Parse(interactionVals[1]);
						if (interactionVals.Length > 2) inter.Exchange = double.Parse(interactionVals[2]);
						if (interactionVals.Length > 3) inter.PairHopping = double.Parse(interactionVals[3]);
					}
					else
					{
						if (interactionVals.Length > 2)
							ThrowEx("Found too many parameters in the interaction.  Only two (Hubbard and exchange) allowed for intersite interaction");

						inter.InterorbitalU = double.Parse(interactionVals[0]);
						inter.Exchange = double.Parse(interactionVals[1]);
					}

					tb.Interactions.Add(inter);
				}
			}

			private void ReadSymmetrySection()
			{
				if (tb.symmetries.Count > 0)
					ThrowEx("Second symmetries section encountered.");

				var symmetries = tb.symmetries;

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
						case "c2(90)":
							n[0, 0] = -1;
							n[1, 0] = -1;
							n[0, 1] = 1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "c2(0)":
							n[0, 0] = 1;
							n[1, 0] = 1;
							n[0, 1] = -1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "c5/6(z)":
							n[0, 1] = 1;
							n[1, 0] = -1;
							n[1, 1] = 1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
							break;
						case "c2/3(z)":
							n[0, 0] = -1;
							n[0, 1] = 1;
							n[1, 0] = -1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
							break;
						case "c1/3(z)":
							n[0, 1] = -1;
							n[1, 0] = 1;
							n[1, 1] = -1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
							break;
						case "c1/6(z)":
							n[0, 0] = 1;
							n[0, 1] = -1;
							n[1, 0] = 1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
							break;
						case "c2(xy[30])":
							n[0, 1] = 1;
							n[1, 0] = 1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "c2(xy[60])":
							n[0, 0] = -1;
							n[0, 1] = 1;
							n[1, 1] = 1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "c2(xy[-60])":
							n[0, 1] = -1;
							n[1, 0] = -1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "c2(xy[-30])":
							n[0, 0] = 1;
							n[0, 1] = -1;
							n[1, 1] = -1;
							n[2, 2] = -1;
							symmetries.Add(n, orbitals);
							break;
						case "s(90)":
							n[0, 0] = 1;
							n[1, 0] = 1;
							n[1, 1] = -1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
							break;
						case "s(0)":
							n[0, 0] = -1;
							n[1, 0] = -1;
							n[1, 1] = 1;
							n[2, 2] = 1;
							symmetries.Add(n, orbitals);
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
							ThrowEx("Invalid Symmetry: {0}", words[0]);
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

					if (tb.sites[i].Equivalent.Contains(orbitals[i]) == false)
						tb.sites[i].Equivalent.Add(orbitals[i]);
					if (tb.sites[orbitals[i]].Equivalent.Contains(i) == false)
						tb.sites[orbitals[i]].Equivalent.Add(i);
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
				reduce.SetRows(tb.lattice.G1, tb.lattice.G2, tb.lattice.G3);
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
						lat.SetColumns(sym * tb.lattice.A1, sym * tb.lattice.A2, sym * tb.lattice.A3);

						lat = reduce * lat;

						s.WriteLine("Lattice vector test...");
						if (CheckLatticeSymmetry(lat) == false)
							goto fail;

						Dictionary<int, int> sitemap = new Dictionary<int, int>();

						s.WriteLine("Generating site map...");

						for (int i = 0; i < tb.sites.Count; i++)
						{
							var site = tb.sites[i];
							Vector3 loc = sym * site.Location;

							int index = tb.Orbitals.FindIndex(tb.lattice, loc);

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
						for (int i = 0; i < tb.hoppings.Count; i++)
						{
							var pair = tb.hoppings[i];

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
						if (newHops.Equals(tb.hoppings) == false)
							goto fail;

						s.WriteLine("Success.");
						validSyms.Add(sym);
						continue;

					fail:
						s.WriteLine("Failed.");
					}

					// now apply symmetries to reduce k-points in kmesh
					int initialKptCount = tb.kmesh.Kpts.Count;

					System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
					watch.Start();

					foreach (var sym in validSyms)
					{
						for (int i = 0; i < tb.kmesh.Kpts.Count; i++)
						{
							KPoint kpt = tb.kmesh.Kpts[i];
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
											   initialKptCount, tb.kmesh.Kpts.Count, watch.ElapsedMilliseconds / 1000);

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
}