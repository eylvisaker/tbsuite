using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;
using TightBinding;

namespace PlotGreen
{
	class PlotGreenProgram
	{
		static void Main(string[] args)
		{
			PlotGreenProgram inst = new PlotGreenProgram();
			inst.Run(args);
		}
		void Run(string[] args)
		{
			string inputfile = TightBinding.MainClass.GetInputFile(args);

			using (StreamWriter w = new StreamWriter("gplot.out"))
			{
				Output.SetFile(w);

				TightBinding.TbInputFile tb = new TightBinding.TbInputFile(inputfile);

				tb.ReadFile();

				//if (File.Exists("green.dat") == false)
				//    throw new FileNotFoundException("The file green.dat is not present.", "green.dat");

				RpaParams p = new RpaParams(0, tb.TemperatureMesh[0], tb.FrequencyMesh[0], tb.MuMesh[0]);
				KptList kmesh = KptList.GenerateMesh(
					tb.Lattice, tb.KMesh.Mesh, null, tb.Symmetries, true, true);

				Matrix[] green = CalcGreenFunction(tb, p, kmesh);

				while (true)
				{
					WriteGreenFunction(tb, green, kmesh);
				};
			}
		}

		private Matrix[] CalcGreenFunction(TbInputFile tb, RpaParams p, KptList kmesh)
		{
			int orbitalCount = tb.Sites.Count;
			Matrix[] retval = new Matrix[kmesh.Kpts.Count];
			TightBinding.TightBinding tbobj = new TightBinding.TightBinding();

			for (int k = 0; k < kmesh.Kpts.Count; k++)
			{
				retval[k] = new Matrix(orbitalCount, orbitalCount);

				Matrix hamilt = tbobj.CalcHamiltonian(tb, kmesh.Kpts[k].Value);
				Matrix vals, vecs;
				hamilt.EigenValsVecs(out vals, out vecs);


				for (int n = 0; n < orbitalCount; n++)
				{
					for (int i = 0; i < orbitalCount; i++)
					{
						for (int j = 0; j < orbitalCount; j++)
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

							Complex g = 1.0 / (p.Frequency + p.ChemicalPotential - wfk.Energy + 
														new Complex(0, p.Temperature));

							retval[k][i, j] += g * coeff;
						}
					}
				}

			}

			return retval;
		}

		private void WriteGreenFunction(TbInputFile tb, Matrix[] green, KptList kmesh)
		{
			WriteGreenFunctionPlane(tb, green, kmesh);
		}

		private void WriteGreenFunctionPlane(TbInputFile tb, Matrix[] green, KptList kmesh)
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
						double lastt = double.MinValue;

						for (int k = 0; k < plane.AllKpts.Count; k++)
						{
							Vector3 kpt = plane.AllKpts[k].Value;
							List<int> orbitalMap;
							double s, t;

							plane.GetPlaneST(plane.AllKpts[k], out s, out t);

							int kindex = kmesh.GetKindex(tb.Lattice, kpt, out orbitalMap, tb.Symmetries);

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

		private RpaParams[] ScanGreenFile(TbInputFile tb)
		{
			List<RpaParams> plist = new List<RpaParams>();

			using (StreamReader r = new StreamReader("green.dat"))
			{
				double temp = 0;
				double mu = 0;
				double freq = 0;

				while (r.EndOfStream == false)
				{
					string line = r.ReadLine();

					if (line.StartsWith("#") == false)
					{
						if (temp != 0)
						{
							plist.Add(
								new RpaParams(0, temp, freq, mu));

							temp = 0;
						}

						continue;
					}
					string[] vals = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

					switch (vals[1])
					{
						case "Temperature:":
							temp = double.Parse(vals[2]);
							break;
						case "Chemical Potential:":
							mu = double.Parse(vals[2]);
							break;
						case "Frequency:":
							freq = double.Parse(vals[2]);
							break;
					}
				}
			}

			return plist.ToArray();
		}

		static readonly char[] seps = new char[] { ' ', '\t' };

		private static Matrix[] ReadGreenFunction(TightBinding.TbInputFile tb,int greenIndex)
		{
			Matrix[] green = new Matrix[tb.KMesh.Kpts.Count];

			using (StreamReader r = new StreamReader("green.dat"))
			{
				int index = 0;
				int lineNumber = 0;

				while (r.EndOfStream == false)
				{
					string line = r.ReadLine();
					lineNumber++;

					if (line.StartsWith("#"))
					{
						if (index > 0)
						{
							greenIndex--;
							if (greenIndex < 0)
								break;
							else
								continue;
						}
						else
							continue;
					}

					string[] vals = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

					if (int.Parse(vals[0]) != index)
						throw new InvalidDataException("Invalid data on line " + lineNumber.ToString() + ".");

					green[index] = new Matrix(tb.Sites.Count, tb.Sites.Count);

					int i = 0;
					int j = 0;

					for (int valIndex = 1; valIndex < vals.Length; valIndex += 2)
					{
						Complex val = Complex.Parse(vals[valIndex], vals[valIndex + 1]);

						green[index][i, j] = val;

						i++;
						if (i >= tb.Sites.Count)
						{
							i = 0;
							j++;
						}
					}

					if (j != tb.Sites.Count)
						throw new InvalidDataException("Invalid data on line " + lineNumber.ToString() + ".");

					index++;

				}
			}

			return green;
		}
	}
}
