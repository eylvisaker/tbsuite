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
			if (args.Length == 0)
			{
				Console.WriteLine("Must specify tight binding input and eigenvalues files on command line.");
				System.Environment.Exit(1);
			}
		
			TightBinding tb = new TightBinding( args[0]);
			
			var argsList = args.ToList();
			argsList.RemoveAt(0);
			
			new BandPath().Run(tb, argsList);
			
		}
		
		void Run (TightBinding tb, List<string> args)
		{
			foreach(var arg in args)
			{
				Console.WriteLine("Reading file " + arg);
		
				string outputfile = arg + ".bands";
				
				CreateBands(tb, outputfile);
			}
		}
		
		void WriteBands (TightBinding tb, KptList kpts, StreamWriter w)
		{
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
				
				NormalizeWeights(weights);
				int count;
				do
				{
					count = CountWeights(weights);
					DropWeakValues(weights);
					NormalizeWeights(weights);
				} while (count != CountWeights(weights));
				
				Console.WriteLine("Using {0} k-points.", count);
				
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
				if (weights[i] < 0.1)
					weights[i] = 0;
			}
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
				}
				
				using (StreamWriter w = new StreamWriter(name))
				{
					WriteBands(tb, kpts, w);
				}
			}
		}
	}
}

