using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace FploBandWeights
{
	class BandWeightsProgram
	{
		public static void Main(string[] args)
		{
			new BandWeightsProgram().Run(args);
		}

		string[] splitString = new string[] { " " };
		string[] doubleSpaceSplit = new string[] { "  " };
		
		List<StateInfo> states = new List<StateInfo>();
		List<int> plots = new List<int>();
		string bandFile = "+band";
		string weightsFile = "+bweights";

		void Run(string[] args)
		{
			if (args.Length == 1)
				weightsFile = args[0];

			LoadStateInfo();

			RunMenu();

			Console.Write("Enter output filename (weights.agr) : ");

			string file = Console.ReadLine();
			if (string.IsNullOrEmpty(file))
				file = "weights.agr";

			Console.WriteLine();

			SaveBandWeights(file);
		}

		private void LoadStateInfo()
		{
			using (var weights = new StreamReader(weightsFile))
			{
				string firstLine = weights.ReadLine();
				string secondLine = weights.ReadLine();

				string[] data = secondLine.Split(doubleSpaceSplit, StringSplitOptions.RemoveEmptyEntries);

				if (data[0].Trim() != "#" ||
					data[1].Trim() != "ik" ||
					data[2].Trim() != "e(k,n)")
				{
					Console.Error.WriteLine("Could not understand input file.");
					Environment.Exit(1);
				}

				for (int i = 3; i < data.Length; i++)
				{
					int index = i - 2;

					StateInfo info = new StateInfo { Index = index, Name = data[i] };

					states.Add(info);
				}

				Console.WriteLine("Found {0} states.", states.Count);


			}
		}

		private void RunMenu()
		{
			bool done = false;

			Console.Clear();

			while (!done)
			{
				Console.WriteLine("Menu:");
				Console.WriteLine();
				Console.WriteLine("  a. Add weight to plot");
				Console.WriteLine("  d. Delete weight to plot");
				Console.WriteLine("  f. Select different band file.");
				Console.WriteLine("  r. Rescan file");
				Console.WriteLine("  x. Exit and save to agr file.");
				Console.WriteLine();

				WritePlots();

				ConsoleKeyInfo key = Console.ReadKey(true);

				Console.WriteLine();

				switch (key.Key)
				{
					case ConsoleKey.A:
						AddWeight();
						break;

					case ConsoleKey.D:
						DeleteWeight();
						break;

					case ConsoleKey.F:
						SelectFile();
						break;

					case ConsoleKey.R:
						RescanFile();
						break;

					case ConsoleKey.X:
						done = true;
						break;
				}


				Console.WriteLine();
			}
		}

		private void RescanFile()
		{
			LoadStateInfo();
		}

		private void SelectFile()
		{
			Console.Write("Enter band file: ");
			bandFile = Console.ReadLine();

			Console.Write("Enter bweights file: ");
			weightsFile = Console.ReadLine();

			RescanFile();
		}

		private void DeleteWeight()
		{
			Console.Write("Enter index to delete (blank to cancel): ");
			string entry = Console.ReadLine().Trim();

			if (string.IsNullOrEmpty(entry))
				return;
			int result;

			if (int.TryParse(entry, out result) == false)
			{
				Console.WriteLine("That is not a valid entry.  Your entry must be numeric.");
				return;
			}

			if (plots.Contains(result))
			{
				plots.Remove(result);
			}
			else
			{
				Console.WriteLine("Could not find the specified index.");
			}
		}

		private void AddWeight()
		{
			Console.Write("Enter filter (blank for none): ");
			string filter = Console.ReadLine();

			Console.WriteLine();
			
			foreach (var state in states)
			{
				if (string.IsNullOrEmpty(filter) == false)
				{
					if (state.Name.Contains(filter) == false)
						continue;
				}

				Console.WriteLine("{0} : {1}", state.Index, state.Name);
			}

			Console.WriteLine();
			Console.Write("Enter index (blank to cancel): ");

			string entry = Console.ReadLine().Trim();

			if (string.IsNullOrEmpty(entry))
				return;

			int[] result = null;
			try
			{
				result = GetIntVals(entry);

			}
			catch
			{
				Console.WriteLine("That is not a valid entry.  Your entry must be numeric.");
				return;
			}

			for (int i = 0; i < result.Length; i++)
			{
				if (plots.Contains(result[i]))
					continue;

				plots.Add(result[i]);
			}
		}

		private int[] GetIntVals(string entry)
		{
			string[] vals = entry.Split(splitString, StringSplitOptions.RemoveEmptyEntries);

			return (from x in vals select int.Parse(x)).ToArray();
		}

		private void WritePlots()
		{
			if (plots.Count != 0)
			{
				Console.WriteLine("Current selected weights: ");
				plots.ForEach(x => { Console.WriteLine("    {0} : {1}", x, State(x).Name); });

				Console.WriteLine();
			}
		}

		private StateInfo State(int index)
		{
			return states.First(x => x.Index == index);
		}


		List<SymmetryPoint> pts = new List<SymmetryPoint>();
		List<AgrDataset> data = new List<AgrDataset>();

		private void SaveBandWeights(string file)
		{
			ReadPoints();
			ReadBands();
			ReadWeights();

			Console.Write("Writing to {0}...", file);
			AgrWriter.Write(file, data, pts);
			Console.WriteLine(" done.");
		}

		private void ReadWeights()
		{
			Console.WriteLine("Reading weights from {0} file.", weightsFile);
			int numBands = 0;
			int numPoints = 0;
			int ptCount = 0;

			using (var r = new StreamReader(weightsFile))
			{
				string first = r.ReadLine();
				if (first.StartsWith("#") == false)
					throw new Exception("Could not read bands file.");

				first = first.Substring(1);
				double[] vals = GetVals(first);

				numPoints = (int)(vals[2] + 0.5);
				numBands = (int)(vals[4] + 0.5);

				r.ReadLine();

				AgrDataset[] data = new AgrDataset[plots.Count];
				for (int i = 0; i < plots.Count; i++)
				{
					data[i] = new AgrDataset();

					data[i].DatasetType = DatasetType.xysize;
					data[i].Legend = State(plots[i]).Name;
		
					data[i].LineStyle = LineStyle.None;
					data[i].Symbol = Symbol.Circle;
					data[i].SymbolColor = (GraceColor)i + 1;
					data[i].SymbolFillColor = (GraceColor)i + 1;
					data[i].SymbolFill = SymbolFill.Solid;
				}

				const double scaleFactor = 1.5;

				for (int i = 0; i < numPoints; i++)
				{
					for (int j = 0; j < numBands; j++)
					{
						string line = r.ReadLine();
						vals = GetVals(line);

						double x = vals[0];
						double y = vals[1];

						for (int k = 0; k < plots.Count; k++)
						{
							int index = plots[k] + 1;

							AgrDataPoint dp = new AgrDataPoint { X = x, Y = y };

							dp.Weight = vals[index] * scaleFactor;

							if (dp.Weight < 0.03)
								continue;

							data[k].Data.Add(dp);
							ptCount += 1;
						}
					}
				}

				this.data.AddRange(data);

			}


			Console.WriteLine("Found {0} bands, with {1} points with weight.", numBands, ptCount);
			Console.WriteLine();

		}

		private void ReadBands()
		{
			Console.WriteLine("Reading bands from {0} file.", bandFile);
			int numBands = 0;
			int numPoints = 0;

			using (var r = new StreamReader(bandFile))
			{
				string first = r.ReadLine();
				if (first.StartsWith("#") == false)
					throw new Exception("Could not read bands file.");

				first = first.Substring(1);
				double[] vals = GetVals(first);

				numPoints = (int)(vals[2] + 0.5);
				numBands = (int)(vals[4] + 0.5);

				AgrDataset[] data = new AgrDataset[numBands];
				for (int j = 0; j < numBands; j++)
				{
					data[j] = new AgrDataset();
				}

				for (int i = 0; i < numPoints; i++)
				{
					// skip comment
					r.ReadLine();

					string line = r.ReadLine();
					vals = GetVals(line);

					for (int j = 0; j < numBands; j++)
					{
						AgrDataPoint dp = new AgrDataPoint();
						dp.X = vals[0];
						dp.Y = vals[j+1];
						
						data[j].Data.Add(dp);
					}
				}

				this.data.AddRange(data);
			}

			Console.WriteLine("Found {0} bands, with {1} k-points.", numBands, numPoints);
			Console.WriteLine();
		}

		private double[] GetVals(string line)
		{
			string[] items = line.Split(splitString, StringSplitOptions.RemoveEmptyEntries);

			return (from x in items select double.Parse(x)).ToArray();
		}

		private void ReadPoints()
		{
			Console.WriteLine("Reading points from +points file.");

			using (var r = new StreamReader("+points"))
			{
				string first = r.ReadLine();
				
				if (first.StartsWith("#") == false)
					throw new Exception("Could not understand +points file.");

				first = first.Substring(1);
				int count = int.Parse(first);

				for (int i = 0; i < count; i++)
				{
					string nameLine = r.ReadLine().Trim();
					string numLine = r.ReadLine().Trim();
					r.ReadLine();
					r.ReadLine();

					string trimedName = nameLine.Substring(3, nameLine.Length - 4);
					string name = trimedName.Trim();
					name = name.Replace("$~", @"\x");
					name = name.Replace("$_", @"\s");
					name = name.Replace("$^", @"\S");
					name = name.Replace("$.", @"\N");

					int index = numLine.IndexOf(' ');

					double val = double.Parse(numLine.Substring(0, index));

					SymmetryPoint pt = new SymmetryPoint { Location = val, Name = name };
					pts.Add(pt);
				}
			}

			Console.WriteLine("Found {0} symmetry points.", pts.Count);
			Console.WriteLine();

		}

	}
}
