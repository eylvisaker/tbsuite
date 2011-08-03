using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	class BandPath
	{
		public static void Main (string[] args)
		{
			Directory.SetCurrentDirectory("/home/eylvisaker/Calculations/rpa/tests");
			
			using (BootStrap b = new BootStrap())
			{
				if (args.Length == 0)
				{
					Console.WriteLine("Must specify tight binding input and eigenvalues files on command line.");
					System.Environment.Exit(1);
				}
			
				string inputfile = b.GetInputFile("Band Path code", "bandpath", args);
				TightBinding tb = new TightBinding(inputfile);
				
				var argsList = args.ToList();
				argsList.RemoveAt(0);
				
				new BandPath().Run(tb, argsList);
			}
		}
		
		void Run (TightBinding tb, List<string> args)
		{
			foreach(var arg in args)
			{
				Console.WriteLine("Reading file " + arg);
						
				CreateBands(tb, arg);
			}
		}
		
		BandTetrahedron GetTetrahedron (TightBinding tb, KPoint kpt, KptList kpts)
		{
			List<Pair<int, double>> lst = new List<Pair<int, double>>();
			
			double[] weights = new double[kpts.Kpts.Count];
			for (int j = 0; j < kpts.Kpts.Count; j++)
			{
				double distance = CalcDistance(tb, kpts.Kpts[j].Value, kpt.Value);
				
				weights[j] = 1 / (distance + 0.00001);
			}
			
			for (int j = 0; j < weights.Length; j++)
			{
				lst.Add(new Pair<int, double>(j, weights[j]));
			}
			
			lst.Sort((x,y) => { return y.Second.CompareTo(x.Second); });
			
			lst.RemoveRange(4, lst.Count - 4);
			List<int> ilist = lst.Select(x => x.First).ToList();
			
			BandTetrahedron retval = new BandTetrahedron(tb, kpt.Value, kpts, ilist);
			
			return retval;
		}

		double CalcDistance(TightBinding tb, Vector3 v1, Vector3 v2)
		{
			Vector3 delta = v1 - v2;
			
			ShiftDelta(ref delta, tb.Lattice.G1);
			ShiftDelta(ref delta, tb.Lattice.G2);
			ShiftDelta(ref delta, tb.Lattice.G3);
			
			return delta.Magnitude;
		}
		
		void ShiftDelta(ref Vector3 delta, Vector3 G)
		{
			if ((delta - G).Magnitude < delta.Magnitude)
			{
				delta -= G;	
			}
			if ((delta + G).Magnitude < delta.Magnitude)
			{
				delta += G;	
			}
		}

		void WriteBands (TightBinding tb, KptList kpts, StreamWriter w)
		{
			int bandCount = kpts.Kpts[0].Wavefunctions.Count;
		
			
			BandTetrahedron tet = null;
			
			for (int i = 0; i < tb.KPath.Kpts.Count; i++)
			{
				var kpt = tb.KPath.Kpts[i];
			
				if (tet == null || tet.Contains(kpt) == false)
				{
					GetTetrahedron(tb, kpt, kpts);
				}
				
				w.Write(i);
				w.Write("   ");
				
				for (int band = 0; band < bandCount; band++)
				{
					double energy = tet.Interpolate(kpt);
										
					w.Write("{0}  ", energy);
				}
				
				w.WriteLine();
			}
		}
		
		int CountWeights(double[] weights)
		{
			return weights.Count(x => x > 0);
		}
		
		void DropWeakValues(double[] weights)
		{
			for (int i = 0; i < weights.Length; i++)
			{
				if (weights[i] < 0.9)
					weights[i] = 0;
			}
			
			
		}
		void MaximizeWeights(double[] weights)
		{
			double total = weights.Max();
			
			for (int i = 0; i < weights.Length; i++)
				weights[i] /= total;
		}
		void NormalizeWeights(List<Pair<int,double>> weights)
		{
			double total = weights.Sum(x => x.Second);
			
			foreach(var x in weights)
			{
				x.Second /= total;	
			}
		}
		void NormalizeWeights(double[] weights)
		{
			double total = weights.Sum();
			
			for (int i = 0; i < weights.Length; i++)
				weights[i] /= total;
		}

		void ParseGrid (int[] grid, int[] shift, string line)
		{
			string[] elements = line.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries );
			
			for (int i = 0; i < 3; i++)
				grid[i] = int.Parse(elements[i]);
			
			for (int i = 0; i < 3; i++)
				shift[i] = int.Parse(elements[3+i]);
		}

		void CreateBands(TightBinding tb, string name)
		{
			using (StreamReader r = new StreamReader(name))
			{
				string line = r.ReadLine();
				
				if (line != "# Grid")
				{
					Console.WriteLine("Not an eigenvalues file!");
					System.Environment.Exit(3);
				}
				
				int[] grid = new int[3];
				int[] shift = new int[3];
				
				
				ParseGrid(grid, shift, line);
				
				if (line != "# Eigenvalues")
				{
					Console.WriteLine("Not an eigenvalues file!");
					System.Environment.Exit(2);
				}
				
				KptList kpts = new KptList();
				kpts.Mesh = grid;
				kpts.Shift = shift;
				
				while (r.EndOfStream == false)
				{
					line = r.ReadLine();
					string[] elements = line.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries );
					
					KPoint kpt = new KPoint(new Vector3(double.Parse(elements[0]),
					                                    double.Parse(elements[1]),
					                                    double.Parse(elements[2])));
					                    
					for (int i = 3; i < elements.Length; i++)
					{
						Wavefunction wfk = new Wavefunction(0);
						wfk.Energy = double.Parse(elements[i]);
						
						kpt.Wavefunctions.Add(wfk);
					}
					
					kpts.Kpts.Add(kpt);
				}
				
				CreateTetrahedronMesh(kpts);
				string outputfile = name + ".bands";
				
				using (StreamWriter w = new StreamWriter(outputfile))
				{
					WriteBands(tb, kpts, w);
				}
			}
		}
		
		void CreateTetrahedronMesh(KptList kpts)
		{
			
		}
	}
}

