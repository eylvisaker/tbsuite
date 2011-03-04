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

				TightBinding tb = new TightBinding( args[0]);
				
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
		
		void WriteBands (TightBinding tb, KptList kpts, StreamWriter w)
		{
			int bandCount = kpts.Kpts[0].Wavefunctions.Count;
			
			double[] weights = new double[kpts.Kpts.Count];
			
			for (int i = 0; i < tb.KPath.Kpts.Count; i++)
			{
				var kpt = tb.KPath.Kpts[i];
			
				w.Write(i);
				w.Write("   ");
				
				for (int j = 0; j < kpts.Kpts.Count; j++)
				{
					Vector3 delta = kpt.Value - kpts.Kpts[j].Value;
					
					double distance = delta.Magnitude;
					
					weights[j] = 1 / (distance + 0.00001);
				}
				
				MaximizeWeights(weights);
				int count;
				do
				{
					count = CountWeights(weights);
					DropWeakValues(weights);
					MaximizeWeights(weights);
				} while (count != CountWeights(weights));
			
				NormalizeWeights(weights);
				
				List<Pair<int, double>> weightList = new List<Pair<int, double>>();
				for (int j = 0; j < weights.Length; j++)
				{
					if (weights[j] > 0)
						weightList.Add(new Pair<int, double>(j, weights[j]));
				}
				
				Console.WriteLine("Using {0} k-points for {1}, {2}, {3}.", weightList.Count,
				                  kpt.Value.X, kpt.Value.Y, kpt.Value.Z );
				
				for (int band = 0; band < bandCount; band++)
				{
					double energy = 0;
					
					foreach(var weight in weightList)
					{
						var srcKpt = kpts.Kpts[weight.First];
						
						energy += srcKpt.Wavefunctions[band].Energy * 
								  weight.Second;
					}
					
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
		void NormalizeWeights(double[] weights)
		{
			double total = weights.Sum();
			
			for (int i = 0; i < weights.Length; i++)
				weights[i] /= total;
		}

		void CreateBands(TightBinding tb, string name)
		{
			using (StreamReader r = new StreamReader(name))
			{
				string line = r.ReadLine();
				if (line != "# Eigenvalues")
				{
					Console.WriteLine("Not an eigenvalues file!");
					System.Environment.Exit(2);
				}
				
				KptList kpts = new KptList();
				
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
				
				string outputfile = name + ".bands";
				
				using (StreamWriter w = new StreamWriter(outputfile))
				{
					WriteBands(tb, kpts, w);
				}
			}
		}
	}
}

