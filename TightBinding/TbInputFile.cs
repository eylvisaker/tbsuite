
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ERY.EMath;

namespace TightBinding
{
	public class TbInputFile : InputReader 
	{
		Lattice lattice;
		SiteList sites;
		HoppingPairList hoppings;
		KptList kpath;
		KptList kmesh;
		List<int> poles = new List<int>();
		double smearing = 0.04;
		
		public Lattice Lattice		{			get { return lattice; }			}
		public SiteList Sites { get { return sites; } }
		public HoppingPairList Hoppings { get { return hoppings; } }
		public KptList KPath { get { return kpath; } }
		public KptList KMesh { get { return kmesh; } }
		public double Smearing { get { return smearing; } }
		public List<int> PoleStates { get { return poles; } }
		
		public TbInputFile(string filename) : base(filename)
		{
		}
		
		protected override void Validate ()
		{
			if (lattice == null)
				ThrowEx(@"""Lattice"" section missing.");
			if (sites == null)
				ThrowEx(@"""Sites"" section missing.");
			if (hoppings == null)
				ThrowEx(@"""Hoppings"" section missing.");
			if (kpath == null)
				kpath = KptList.DefaultPath(lattice);
			
					
			if (sites.Count == 0)
				ThrowEx(@"There are no sites.");
			if (hoppings.Count == 0)
				ThrowEx(@"There are no hoppings.");
			
			foreach(HoppingPair h in hoppings)
			{
				if (h.Left > sites.Count || h.Right > sites.Count)
					ThrowEx(string.Format(@"The hopping {0} to {1} was specified, but there are only {2} sites.",
					                      h.Left, h.Right, sites.Count));
			}
			
			if (kmesh != null)
				ApplySymmetries();
		}
 
		protected override void ReadSection (string sectionName)
		{
			switch(sectionName)
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
				
			case "KPath":
				ReadKPathSection();
				break;
				
			case "KMesh":
				ReadKMeshSection();
				break;
				
			case "Poles":
				ReadPolesSection();
				break;
				
			default:
				ThrowEx("Unrecognized section " + sectionName);
				break;
			}
		}
		void ReadPolesSection()
		{
			string[] vals = Line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
			
			foreach(string v in vals)
				poles.Add(int.Parse(v)-1);
		}
		void ReadKPathSection()
		{
			if (kpath != null)
				ThrowEx("KPath found twice.");
			
			Vector3 lastKpt = Vector3.Zero;
			char[] array = new char[] {' '};
			const double ptScale = 100;
			
			kpath = new KptList();
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
				
				kpath.AddPts(lastKpt, kpt, Math.Max((int)(ptScale * length), 1));
				kpath.Kpts[kpath.Kpts.Count-1].Name = name;
				
				lastKpt = kpt;
				
				ReadNextLine();
			}
		}
		void ReadKMeshSection()
		{
			char[] array = new char[] {' '};
			string[] vals = Line.Split(array, StringSplitOptions.RemoveEmptyEntries);
			
			if (vals.Length != 3)
				ThrowEx("Need exactly 3 integers for kmesh.");
			if (kmesh != null)
				ThrowEx("Second kmesh found.");
			
			kmesh = KptList.GenerateMesh(lattice, int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]));
			
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
				
				Console.WriteLine(Line);
					
				if (values.Length != 2) 
					ThrowEx("Could not understand hopping pair.");
				
				int left = int.Parse(values[0])-1;
				int right = int.Parse(values[1])-1;
				
				HoppingPair p = new HoppingPair(left, right);
				hoppings.Add(p);
				
				Console.WriteLine("Reading hoppings for {0}-{1}", left+1, right+1);
				
				ReadNextLine();
				
				while (LineType == LineType.Hopping || LineType == LineType.Numeric)
				{
					List<string> vals = new List<string>();
					vals.AddRange(Line.Replace('\t', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

					if (vals[0] == "T=")	vals.RemoveAt(0);
					if (vals[3] == "hop=")	vals.RemoveAt(3);
					
					double value = double.Parse(vals[vals.Count - 1]);

					Vector3 loc = new Vector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));

					HoppingValue v = new HoppingValue();
					v.Value = value;
					v.R = loc;
						
					p.Hoppings.Add(v);
					
					ReadNextLine();
				}
				
				Console.WriteLine("Count: {0}", p.Hoppings.Count);
					
			}
		}
		
		IEnumerable<Matrix> GetPossibleSymmetries()
		{
			Matrix m = Matrix.Identity(3);
			
			// identity
			yield return m;
			
			// inversion
			yield return -1 * m;
			
			// reflections
			m[0,0] = -1;
			yield return m;
			
			m[0,0] = 1;
			m[1,1] = -1;
			yield return m;
			
			m[1,1] = 1;
			m[2,2] = -1;
			yield return m;
			
			// rotations
			m = Matrix.Zero(3);
			
			m[1,0] = 1;
			m[0,1] = 1;
			m[2,2] = 1;
			yield return m;
			
			m = Matrix.Zero(3);
			
			m[2,0] = 1;
			m[0,2] = 1;
			m[1,1] = 1;
			yield return m;
			
			m = Matrix.Zero(3);
			
			m[2,1] = 1;
			m[1,2] = 1;
			m[0,0] = 1;
			yield return m;
			
			// rotation/reflections
			yield return new Matrix(3, 3, 
			                        0, -1, 0,
			                        1,  0, 0,
			                        0,  0, 1);
			             
			yield return new Matrix(3, 3, 
			                        0,  1, 0,
			                        -1, 0, 0,
			                        0,  0, 1);

			yield return new Matrix(3, 3, 
			                        0,  1, 0,
			                        1,  0, 0,
			                        0,  0, -1);

		}
		
		void ApplySymmetries()
		{
			List<Matrix> validSyms = new List<Matrix>();
			Matrix reduce = new Matrix(3,3);
			reduce.SetRows(lattice.G1, lattice.G2, lattice.G3);
			int symIndex = 0;
			
			using (StreamWriter s = new StreamWriter("syms"))
			{			
				foreach(var sym in GetPossibleSymmetries())
				{
					symIndex ++;
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
						
						foreach(var hop in pair.Hoppings)
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
				
				foreach(var sym in validSyms)
				{
					for(int i = 0; i < kmesh.Kpts.Count; i++)
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
				
				Console.WriteLine(fmt);
				s.WriteLine(fmt);
			}
		}
		bool CheckLatticeSymmetry(Matrix lat)
		{	
			for (int col = 0; col < 3; col++)
			{
				int count = 0;
				
				for (int row = 0; row < 3; row ++)
				{
					double x = lat[row,col].RealPart;
				
					// skip zeros
					if (Math.Abs(x) < 1e-6)
						continue;
					
					if (Math.Abs(x) - 1 > 1e-6)
						return false;
					else
						count ++;
					
				}
				
				if (count != 1)
					return false;
			}
			
			return true;
		}
	}
}
