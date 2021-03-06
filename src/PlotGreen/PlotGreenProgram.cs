﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	class PlotGreenProgram
	{
		static void Main(string[] args)
		{
			using (BootStrap b = new BootStrap())
			{
				string inputfile = b.GetInputFile("Green's function generator", "g", args);

				PlotGreenProgram inst = new PlotGreenProgram();
				inst.Run(inputfile);
			}
		}
		void Run(string inputfile)
		{
			using (StreamWriter w = new StreamWriter("gplot.out"))
			{
				Output.SetFile(w);

				TightBindingSuite.TightBinding tb = new TightBindingSuite.TightBinding();
				tb.LoadTB(inputfile);

				RpaParams p = new RpaParams(0, Vector3.Zero, tb.TemperatureMesh[0], tb.FrequencyMesh[0], tb.MuMesh[0]);
				KptList kmesh = KptList.GenerateMesh(
					tb.Lattice, tb.KMesh.Mesh, null, tb.Symmetries, true);

				Matrix[] green = CalcGreenFunction(tb, p, kmesh);

				while (true)
				{
					WriteGreenFunction(tb, green, kmesh);
				};
			}
		}

		private Matrix[] CalcGreenFunction(TightBindingSuite.TightBinding tb, RpaParams p, KptList kmesh)
		{
			int orbitalCount = tb.Orbitals.Count;
			Matrix[] retval = new Matrix[kmesh.Kpts.Count];
			
			Complex denomFactor = new Complex(0, p.Temperature);

			for (int k = 0; k < kmesh.Kpts.Count; k++)
			{
				retval[k] = new Matrix(orbitalCount, orbitalCount);

				Matrix hamilt = tb.CalcHamiltonian(kmesh.Kpts[k].Value);
				Matrix vals, vecs;
				hamilt.EigenValsVecs(out vals, out vecs);


				for (int i = 0; i < orbitalCount; i++)
				{
					for (int j = 0; j < orbitalCount; j++)
					{
						for (int n = 0; n < orbitalCount; n++)
						{
							var wfk = new Wavefunction(orbitalCount);

							wfk.Energy = vals[n, 0].RealPart;

							for (int c = 0; c < vecs.Rows; c++)
							{
								wfk.Coeffs[c] = vecs[c, n];
							}

							Complex coeff =
								wfk.Coeffs[i].Conjugate() *
								wfk.Coeffs[j];

							Complex g = 1.0 / (p.Frequency + p.ChemicalPotential - wfk.Energy + denomFactor);

							retval[k][i, j] += g * coeff;
						}
					}
				}

			}

			return retval;
		}

		private void WriteGreenFunction(TightBindingSuite.TightBinding tb, Matrix[] green, KptList kmesh)
		{
			WriteGreenFunctionPlane(tb, green, kmesh);
		}

		private void WriteGreenFunctionPlane(TightBindingSuite.TightBinding tb, Matrix[] green, KptList kmesh)
		{
			Console.WriteLine("Output as a plane.");
			Console.WriteLine();

			Console.WriteLine("Enter vectors as a series of three numbers, like: 1 1 0");
			Console.WriteLine("Only integer values need be used.");
			Console.WriteLine();

			Console.Write("Enter first vector: ");
			string first = Console.ReadLine();
			if (string.IsNullOrEmpty(first))
				return;

			Console.Write("Enter second vector: ");
			string second = Console.ReadLine();

			Vector3 sdir = Vector3.Parse(first);
			Vector3 tdir = Vector3.Parse(second);
			Vector3 udir = Vector3.CrossProduct(sdir, tdir);

			Console.Write("Enter origin point: ");
			string origin = Console.ReadLine();
			Vector3 orig = Vector3.Parse(origin);

			Vector3 closestKpt = Vector3.Zero;
			double closestDistance = 999999999;

			foreach (var kpt in kmesh.AllKpts)
			{
				double distance = (kpt.Value - orig).MagnitudeSquared;

				if (distance < closestDistance)
				{
					closestKpt = kpt.Value;
					closestDistance = distance;
				}
			}

			if (closestDistance > 1e-6)
			{
				Console.WriteLine("Using closest k-point to specified origin: {0}.", closestKpt);
				orig = closestKpt;
				sdir += closestKpt;
				tdir += closestKpt;
			}

			KptList plane = KptList.GeneratePlane(
				tb.Lattice, new Vector3[] { orig, sdir, tdir }, tb.Symmetries, kmesh);

			string tr_filename = string.Format("green.tr.pln");
			StreamWriter tr = new StreamWriter(tr_filename);

			double lastt = double.MinValue;

			for (int k = 0; k < plane.AllKpts.Count; k++)
			{
				Complex trValue = new Complex();

				for (int i = 0; i < green[k].Rows; i++)
				{
					trValue += green[k][i, i];
				}
				Vector3 kpt = plane.AllKpts[k].Value;
				List<int> orbitalMap;
				double s, t;

				plane.GetPlaneST(plane.AllKpts[k], out s, out t);

				int kindex = kmesh.IrreducibleIndex(kpt, tb.Lattice, tb.Symmetries, out orbitalMap);

				if (Math.Abs(t - lastt) > 1e-6)
				{
					tr.WriteLine();
					lastt = t;
				}

				tr.WriteLine("{0}\t{1}\t{2}", s, t, -trValue.ImagPart);
			}
			tr.Close();


			for (int i = 0; i < green[0].Rows; i++)
			{
				for (int j = 0; j < green[0].Columns; j++)
				{
					string re_filename = string.Format("green.re.{0}.{1}.pln", i,j);
					string im_filename = string.Format("green.im.{0}.{1}.pln", i, j);
					string mag_filename = string.Format("green.mag.{0}.{1}.pln", i, j);

					StreamWriter rew = new StreamWriter(re_filename);
					StreamWriter imw = new StreamWriter(im_filename);
					StreamWriter mag = new StreamWriter(mag_filename);

					try
					{
						lastt = double.MaxValue;

						for (int k = 0; k < plane.AllKpts.Count; k++)
						{
							Vector3 kpt = plane.AllKpts[k].Value;
							List<int> orbitalMap;
							double s, t;

							plane.GetPlaneST(plane.AllKpts[k], out s, out t);

							int kindex = kmesh.IrreducibleIndex(kpt, tb.Lattice, tb.Symmetries, out orbitalMap);

							if (Math.Abs(t - lastt) > 1e-6)
							{
								rew.WriteLine();
								imw.WriteLine();
								mag.WriteLine();

								lastt = t;
							}

							rew.WriteLine("{0}\t{1}\t{2}", s, t, green[kindex][i, j].RealPart);
							imw.WriteLine("{0}\t{1}\t{2}", s, t, -green[kindex][i, j].ImagPart);
							mag.WriteLine("{0}\t{1}\t{2}", s, t, green[kindex][i, j].Magnitude);
							
						}
					}
					finally
					{
						rew.Dispose();
						imw.Dispose();
						mag.Dispose();
					}
				}
			}
		}

		private int GetGreenIndex(RpaParams[] plist)
		{
			if (plist.Length == 1)
				return 0;

			Console.WriteLine();
			Console.WriteLine("Found {0} Green's functions.", plist.Length);

			for(;;)
			{
				Console.Write("Enter index to use (1-{0}): ", plist.Length);
				int index;

				if (int.TryParse(Console.ReadLine(), out index) == false)
					continue;

				Console.WriteLine();
				Console.WriteLine("Chose Green's function index {0}.", index);
				Console.WriteLine("   Temperature: {0}", plist[index].Temperature);
				Console.WriteLine("   Frequency: {0}", plist[index].Frequency);
				Console.WriteLine("   Mu: {0}", plist[index].ChemicalPotential);
				Console.WriteLine();
				Console.Write("Is this ok (y/n)? ");

				var key = Console.ReadKey();
			
				if (key.Key == ConsoleKey.Y)
				{
					return index;
				}
			} 
		}

	}
}
