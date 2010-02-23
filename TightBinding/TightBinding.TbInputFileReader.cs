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
				if (tb.orbitals == null)
					ThrowEx(@"""Sites"" section missing.");
				if (tb.hoppings == null)
					ThrowEx(@"""Hoppings"" section missing.");
				if (tb.kpath == null)
					tb.kpath = KptList.DefaultPath();
				if (tb.kgrid == null || tb.kgrid[0] == 0 || tb.kgrid[1] == 0 || tb.kgrid[2] == 0)
					ThrowEx(@"KMesh was not defined properly.");

				if (tb.orbitals.Count == 0)
					ThrowEx(@"There are no sites.");
				if (tb.hoppings.Count == 0)
					ThrowEx(@"There are no hoppings.");

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

				for (int i = 0; i < 3; i++)
				{
					if (tb.qgrid[i] != 0)
					{
						int intDiv = tb.kgrid[i] / tb.qgrid[i];
						double dDiv = tb.kgrid[i] / (double)tb.qgrid[i];

						if (dDiv != intDiv)
						{
							ThrowEx("QGrid is not commensurate with KGrid.");
						}
					}
				}
				foreach (HoppingPair h in tb.hoppings)
				{
					if (h.Left >= tb.orbitals.Count || h.Right >= tb.orbitals.Count)
						ThrowEx(string.Format(@"The hopping {0} to {1} was specified, but there are only {2} sites.",
											  h.Left+1, h.Right+1, tb.orbitals.Count));

					Vector3 localDiff = tb.Orbitals[h.Right].Location - tb.Orbitals[h.Left].Location;

					foreach (var hop in h.Hoppings)
					{
						Vector3 test = hop.R - localDiff;

						test = MoveNearGamma(test);
						if (test.Magnitude > 1e-4)
						{
							ThrowEx(string.Format("The hopping between orbitals {0} and {1} along vector {2} does not seem to match the position of the orbitals.",
								h.Left + 1, h.Right + 1, hop.R.ToString("0.0000")));
						}
					}
				}
				
				bool addedPairs = false;
				for (int i = 0; i < tb.orbitals.Count; i++)
				{
					for (int j = i; j < tb.orbitals.Count; j++)
					{
						HoppingPair p = tb.hoppings.Find(i, j);
							
						if (p == null)
						{
							if (i == j)
								ThrowEx(string.Format("There is no hopping for the {0} channel.", i+1));
							
							Output.WriteLine("WARNING: Could not find hopping pair {0}-{1}.", i+1,j+1);
							addedPairs = true;
							
							tb.hoppings.Add(new HoppingPair(i,j));
							tb.hoppings.Add(new HoppingPair(j,i));
						}
					}
				}
				if (addedPairs)
				{
					Output.WriteLine("Added missing hopping pairs.  Check to make sure this behavior is correct.");
					Output.WriteLine();
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

				DetectSpaceGroup(tb);
				ValidateSymmetries(tb.kgrid, tb.SpaceGroup.Symmetries);

			}


			private void ValidateSymmetries(int[] kgrid, SymmetryList syms)
			{
				Vector3 gridVector = new Vector3(kgrid[0], kgrid[1], kgrid[2]);

				//foreach (var symmetry in syms)
				//{
				//    Vector3 grid2 = symmetry.Value * gridVector;
				//    for (int gi = 0; gi < 3; gi++)
				//        grid2[gi] = Math.Abs(grid2[gi]);

				//    if (grid2 != gridVector)
				//    {
				//        ThrowEx("The kmesh specified is not compatible with the symmetry of the system.");
				//    }
				//}
			}

			private Vector3 MoveNearGamma(Vector3 v)
			{
				for (int i = 0; i < 3; i++)
				{
					while (Math.Abs(v[i] - 1) < Math.Abs(v[i]))
					{
						v[i] -= 1;
					}
					while (Math.Abs(v[i] + 1) < Math.Abs(v[i]))
					{
						v[i] += 1;
					}
				}

				return v;
			}

			protected override void PostProcess()
			{
				GenerateKmesh();

			}

			private void GenerateKmesh()
			{
				tb.mAllKmesh = KptList.GenerateMesh(tb.kgrid, tb.shift, false);
				tb.mKmesh = tb.mAllKmesh.CreateIrreducibleMesh(tb.SpaceGroup.Symmetries);

				Output.WriteLine("Applied {0} symmetries to get {1} irreducible kpoints from {2}.",
					tb.SpaceGroup.Symmetries.Count, tb.mKmesh.Kpts.Count, tb.mAllKmesh.Kpts.Count);

				using (StreamWriter writer = new StreamWriter("kpts"))
				{
					for (int i = 0; i < tb.mAllKmesh.Kpts.Count; i++)
					{
						Vector3 red = tb.lattice.ReciprocalReduce(tb.mAllKmesh.Kpts[i].Value);

						writer.WriteLine("{0}     {1}", i, red);
					}
				}

				if (tb.setQplane)
				{
					tb.mAllQplane = KptPlane.GeneratePlane(tb.lattice, tb.qplaneDef, tb.qgrid, null);
					tb.mQplane = tb.mAllQplane.CreateIrreduciblePlane(tb.SpaceGroup.Symmetries);

					Output.WriteLine("Found {0} irreducible qpoints in the plane of {1} qpoints.",
						tb.mQplane.Kpts.Count, tb.mAllQplane.Kpts.Count);

					using (StreamWriter writer = new StreamWriter("qpts"))
					{
						for (int i = 0; i < tb.mQplane.Kpts.Count; i++)
						{
							Vector3 red = tb.lattice.ReciprocalReduce(tb.mQplane.Kpts[i].Value);

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

					case "Symmetry":
						ReadSymmetrySection();

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

			private void ReadSymmetrySection()
			{
				bool deprecated = false;

				if (Options.ContainsKey("disable"))
				{
					tb.disableSymmetries = true;

					if (Options.Count > 1)
						deprecated = true;
				}
				else if (Options.Count > 0)
					deprecated = true;


				if (deprecated)
				{
					Output.WriteLine("Deprecated symmetry options present.");
					Output.WriteLine("Remove them or suffer the consequences.");
				}
				
			}

			private void ReadQPlaneSection()
			{
				bool reduced = false;

				reduced = Options.ContainsKey("reduced");
				
				for (int i = 0; i < 3; i++)
				{
					tb.qgrid[i] = int.Parse(LineWords[i]);
				}

				if (Options.ContainsKey("skip"))
					tb.SkipQPlaneLines = true;
				
				ReadNextLine();

				for (int i = 0; i < 3; i++)
				{
					tb.qplaneDef[i] = Vector3.Parse(Line);
					
					if (reduced)
					{
						tb.qplaneDef[i] = tb.Lattice.ReciprocalExpand(tb.qplaneDef[i]);
					}
					
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

				bool reduced = false;

				reduced = Options.ContainsKey("reduced");

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

					Vector3 kpt =Vector3.Parse(text);
					
					if (reduced == false)
					{
						kpt = tb.Lattice.ReciprocalReduce(kpt);
					}

					double length = (tb.Lattice.ReciprocalExpand(kpt) - tb.Lattice.ReciprocalExpand(lastKpt)).Magnitude;

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

				Output.WriteLine("Direct lattice vectors:");
				Output.WriteLine("    a1: {0}", tb.lattice.A1);
				Output.WriteLine("    a2: {0}", tb.lattice.A2);
				Output.WriteLine("    a3: {0}", tb.lattice.A3);
				Output.WriteLine();

				Output.WriteLine("Reciprocal lattice vectors:");
				Output.WriteLine("    g1: {0}", tb.lattice.G1);
				Output.WriteLine("    g2: {0}", tb.lattice.G2);
				Output.WriteLine("    g3: {0}", tb.lattice.G3);
				Output.WriteLine();

			}
			void ReadOrbitalsSection()
			{
				if (tb.orbitals != null)
					ThrowEx("Multiple sites sections found.");

				tb.orbitals = new OrbitalList();
				bool reduced = false;

				reduced = Options.ContainsKey("reduced");


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

					if (!reduced)
						loc = tb.Lattice.DirectReduce(loc);

					string name = vals[3];
					string sitename = vals[4];
					string localsym = vals[5];
					string interactionGroup = vals[6];

					tb.orbitals.Add(new Orbital(loc, name, sitename, localsym, interactionGroup));

					ReadNextLine();
				}
			}

			void ReadHoppingsSection()
			{
				if (tb.hoppings != null)
					ThrowEx("Multiple hoppings sections found.");

				if (LineType != LineType.NewSubSection)
					ThrowEx("Hoppings section must start with :..: delimited section.");

				bool reduced = false;

				reduced = Options.ContainsKey("reduced");

				tb.hoppings = new HoppingPairList();

				while (!EOF && LineType != LineType.NewSection)
				{
					string[] values = ReadSubSectionParameters();

					int left = int.Parse(values[0]) - 1;
					int right = int.Parse(values[1]) - 1;

					HoppingPair p = new HoppingPair(left, right);
					tb.hoppings.Add(p);

					ReadNextLine();

					while (LineType == LineType.Hopping || LineType == LineType.Numeric)
					{
						List<string> vals = new List<string>();
						vals.AddRange(Line.Replace('\t', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

						if (vals[0] == "T=") vals.RemoveAt(0);
						if (vals[3] == "hop=") vals.RemoveAt(3);

						double value = double.Parse(vals[vals.Count - 1]);

						Vector3 loc = new Vector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));
						if (reduced == false)
							loc = tb.lattice.DirectReduce(loc);

						HoppingValue v = new HoppingValue();
						v.Value = value;
						v.R = loc;

						p.Hoppings.Add(v);

						ReadNextLine();
					}

				}
			}

			private void ReadInteractionSection()
			{
				if (tb.Interactions != null)
					ThrowEx("Multiple Interaction sections found.");

				tb.Interactions = new InteractionList();

				if (Options.ContainsKey("adjust"))
				{
					double val;

					val = Options["adjust"] ?? 0.001;

					if (val <= 0 || val >= 1)
					{
						ThrowEx("Interaction adjustment must be between 0 and 1, and should be close to zero.");
					}

					tb.Interactions.MaxEigenvalue = 1 - val;
					tb.Interactions.AdjustInteractions = true;
				}

				bool reduced = Options.ContainsKey("reduced");
				
				while (!EOF && LineType != LineType.NewSection)
				{
					if (LineType != LineType.NewSubSection)
					{
						ThrowEx("Could not understand contents of interaction section.");
					}

					string[] values = ReadSubSectionParameters();

					InteractionPair inter = new InteractionPair(tb.orbitals, values[0], values[1]);
					
					if (inter.OrbitalsLeft.Count == 0) ThrowEx("Could not identify interaction group \"" + values[0] + "\".");
					if (inter.OrbitalsRight.Count == 0) ThrowEx("Could not identify interaction group \"" + values[1] + "\".");

					ReadNextLine();
					string[] interactionVals = Line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					double[] vals = interactionVals.Select(x => double.Parse(x)).ToArray();

					ReadNextLine();
					
					while (!EOF && LineType != LineType.NewSection && LineType != LineType.NewSubSection)
					{
					
						Vector3 vec = Vector3.Parse(Line);
						if (reduced == false)
							vec = tb.lattice.DirectReduce(vec);

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

			private void DetectSpaceGroup(TightBinding tb)
			{
				SymmetryList syms = new SymmetryList();
				Matrix vecs = new Matrix(3, 3);
				Matrix vinv;

				for (int i = 0; i < 3; i++)
					vecs.SetColumn(i, tb.Lattice.LatticeVector(i));

				vinv = vecs.Invert();

				using (StreamWriter s = new StreamWriter("syms"))
				{
					for (int i = 0;  i < SpaceGroup.AllPrimitiveSymmetries.Count; i++)
					{
						Symmetry sym = SpaceGroup.AllPrimitiveSymmetries[i];

						s.WriteLine("Testing symmetry " + sym.Name + ":");
						s.WriteLine(sym.Value);

						Matrix symValue = vecs * sym.Value * vinv;

						if (CheckLatticeSymmetry(tb.lattice, symValue) == false)
							goto fail;
						
						s.WriteLine("Passed lattice vector symmetry.");
						s.WriteLine("Testing orbital symmetry.");

						OrbitalMap orbitalMap;

						if (CheckOrbitalSymmetry(tb, sym, out orbitalMap) == false)
							goto fail;

						if (CheckHoppingSymmetry(tb, sym, orbitalMap) == false)
							goto fail;

						syms.Add(sym);
						continue;
					fail:
						s.WriteLine("Failed.");
					}
				}

				SpaceGroup sp = SpaceGroup.IdentifyGroup(syms);

				Output.WriteLine();
				Output.WriteLine("Found space group {0}:  {1}", sp.Number, sp.Name);

				if (tb.disableSymmetries)
				{
					sp = SpaceGroup.LowestSymmetryGroup;
					Output.WriteLine("But symmetries are disabled, so instead using {0}:  {1}", sp.Number, sp.Name);
				}

				Output.WriteLine();

				tb.SpaceGroup = sp;
			}

			private bool CheckHoppingSymmetry(TightBinding tb, Symmetry sym, OrbitalMap orbitalMap)
			{
				HoppingPairList newHopPairs = new HoppingPairList();

				foreach (var hopPair in tb.hoppings)
				{
					HoppingPair p = new HoppingPair(orbitalMap[hopPair.Left], orbitalMap[hopPair.Right]);

					foreach (var hop in hopPair.Hoppings)
					{
						HoppingValue val = new HoppingValue();
						val.R = sym.Value * hop.R;
						val.Value = hop.Value;

						// TODO: need to multiply value by -1 if one basis orbital changes sign!

						p.Hoppings.Add(val);
					}

					newHopPairs.Add(p);
				}

				if (newHopPairs.Equals(tb.hoppings))
					return true;
				else
					return false;

			}

			private bool CheckOrbitalSymmetry(TightBinding tb, Symmetry sym, out OrbitalMap orbitalMap)
			{
				orbitalMap = new OrbitalMap(tb.Orbitals.Count);

				for (int i = 0; i < tb.orbitals.Count; i++)
				{
					Orbital orb = tb.orbitals[i];

					Vector3 newLoc = sym.Value * orb.Location;
					newLoc += sym.Translation;
					orbitalMap[i] = i;

					bool valid = false;
					for (int j = 0; j < tb.orbitals.Count; j++)
					{
						Vector3 diff = newLoc - tb.orbitals[j].Location;

						if (VectorIsInteger(diff))
						{
							// now check to see if orbital transforms correctly.
							if (CheckSymmetryDesignation(orb, tb.orbitals[j], sym) == false)
								continue;

							valid = true;
							orbitalMap[i] = j;
							break;
						}
					}

					if (valid == false)
						return false;

				}

				return true;
			}

			private bool CheckSymmetryDesignation(Orbital orb, Orbital orbital, Symmetry sym)
			{
				OrbitalDesignation odl = ODHelper.FromString(orb.LocalSymmetry);
				OrbitalDesignation odr = ODHelper.FromString(orbital.LocalSymmetry);

				OrbitalDesignation trans = ODHelper.TransformUnderSymmetry(odl, sym.Value);

				if (odr == trans)
					return true;
				else
					return false;
			}

			bool CheckLatticeSymmetry(Lattice lat, Matrix sym)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector3 test = sym * lat.LatticeVector(i);
					Vector3 red = lat.DirectReduce(test);

					if (VectorIsInteger(red) == false)
						return false;
				}

				return true;
			}

			private static bool VectorIsInteger(Vector3 red)
			{
				for (int j = 0; j < 3; j++)
				{
					while (Math.Abs(red[j] - 1) < Math.Abs(red[j])) red[j] -= 1;
					while (Math.Abs(red[j] + 1) < Math.Abs(red[j])) red[j] += 1;

					if (Math.Abs(red[j]) > 1e-5)
					{
						return false;
					}
				}

				return true;
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

						for (int i = 0; i < tb.orbitals.Count; i++)
						{
							var site = tb.orbitals[i];
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
					int initialKptCount = tb.mAllKmesh.Kpts.Count;

					System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
					watch.Start();

					foreach (var sym in validSyms)
					{
						for (int i = 0; i < tb.mAllKmesh.Kpts.Count; i++)
						{
							KPoint kpt = tb.mAllKmesh.Kpts[i];
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
											   initialKptCount, tb.mAllKmesh.Kpts.Count, watch.ElapsedMilliseconds / 1000);

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

			private void StoreEquivalentOrbitals(List<int> orbitals)
			{
				for (int i = 0; i < orbitals.Count; i++)
				{
					if (orbitals[i] == i)
						continue;

					if (tb.orbitals[i].Equivalent.Contains(orbitals[i]) == false)
						tb.orbitals[i].Equivalent.Add(orbitals[i]);
					if (tb.orbitals[orbitals[i]].Equivalent.Contains(i) == false)
						tb.orbitals[orbitals[i]].Equivalent.Add(i);
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


			private double GetDist(Vector3 a, Vector3 b)
			{
				return (a - b).MagnitudeSquared;
			}
		}
	}
}