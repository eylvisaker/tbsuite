using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ERY.EMath;

namespace FploWannierConverter
{
	class WannierConverter
	{
		static void Main(string[] args)
		{
			new WannierConverter().Run(args);
		}

		private void Run(string[] args)
		{
			WannierData data = ReadData();
			WriteData(data);
		}

		
		private WannierData ReadData()
		{
			WannierData retval = new WannierData();

			InitRegex();

			using (StreamReader reader = new StreamReader("opendxWF.dx"))
			{
				while (reader.EndOfStream == false)
				{
					string line = ReadNextLine(reader);

					if (line.StartsWith("object"))
					{
						ReadDXObject(reader, line, retval);
					}
					else
						throw new InvalidDataException("Could not understand line.");
				}
			}

			using (StreamReader reader = new StreamReader("out"))
			{
				ReadFploInfo(reader, retval);
			}
			return retval;
		}

		private void ReadFploInfo(StreamReader reader, WannierData retval)
		{
			Fplo8Data data = new Fplo8Data();

			int count = 0;

			while (reader.EndOfStream == false)
			{
				string line = reader.ReadLine();

				if (line.StartsWith("lattice vectors"))
				{
					data.ReadLatticeVectors(reader, data);
				}
				if (line.StartsWith("Number of sites :"))
				{
					count = int.Parse(line.Substring(line.Length - 3));
				}
				if (line.StartsWith("No.  Element WPS"))
				{
					data.ReadAtomSites(reader, data, count);
				}
			}

			data.FindAtoms(retval);
		}

		private static string ReadNextLine(StreamReader reader)
		{
			while (reader.EndOfStream == false)
			{
				string line = reader.ReadLine();

				if (line.Contains("#"))
					line = line.Substring(0, line.IndexOf("#"));

				if (string.IsNullOrEmpty(line))
					continue;

				return line;
			}

			return string.Empty;
		}
		private static void ReadToComment(StreamReader reader)
		{
			while (reader.EndOfStream == false)
			{
				string line = reader.ReadLine();
				
				if (line.StartsWith("#"))
					return;
			}
		}

		private void InitRegex()
		{
			reg = new Regex(@"[a-zA-Z0-9]+|""[^""]*""", RegexOptions.Compiled);
		}

		Regex reg;

		private void ReadDXObject(StreamReader reader, string line, WannierData retval)
		{
			string[] list = GetMatches(line);

			System.Diagnostics.Debug.Assert(list[0] == "object");

			int shape = -1;
			int count = -1;

			for (int i = 2; i < list.Length; i++)
			{
				if (list[i] == "shape")
				{
					shape = int.Parse(list[i + 1]);
				}
				if (list[i] == "data")
				{
					count = int.Parse(list[i - 1]);
				}
			}

			System.Diagnostics.Debug.Assert(list[1].StartsWith("\"") && list[1].EndsWith("\""));

			string name = StripQuotes(list[1]);
			int number;

			string theclass = list[3];

			if (int.TryParse(name.Substring(name.Length - 3, 3), out number))
			{
				name = name.Substring(0, name.Length - 3);
				number--;
			}

			switch (theclass)
			{
				case "array":
					switch (name)
					{
						case "wfnames":
							Console.WriteLine("Reading names");
							ReadWFnames(reader, retval, count);
							break;

						case "wfdata":
							Console.WriteLine("Reading data for WF {0}", number);
							ReadWFdata(reader, retval, count, number);
							break;

						case "atompositions":
							Console.WriteLine("Reading atom positions for WF {0}", number);
							ReadAtomPositions(reader, retval, count, number);
							break;

						case "dataradius":
						case "datacolor":
						case "atom":
							Console.WriteLine("Skipping {0} {1}", name, number);
							ReadToComment(reader);
							break;
					}
					break;

				case "gridpositions":
					Console.WriteLine("Reading grid positions");
					ReadPositions(reader, retval, list);
					break;
				case "series":
				case "gridconnections":
				case "field":
					Console.WriteLine("Skipping {0}", name);
					ReadToComment(reader);
					break;
			}
		}

		private void ReadAtomPositions(StreamReader reader, WannierData retval, int count, int number)
		{
			if (number > 0)
			{
				ReadToComment(reader);
				return;
			}

			for (int i = 0; i < count; i++)
			{
				Vector3 p = Vector3.Parse(ReadNextLine(reader));

				Atom a = new Atom();
				a.Position = p;

				retval.Atoms.Add(a);
			}

			ReadToComment(reader);
		}

		private void ReadPositions(StreamReader reader, WannierData retval, string[] list)
		{
			int len = list.Length;
			int xgrid = int.Parse(list[len - 3]);
			int ygrid = int.Parse(list[len - 2]);
			int zgrid = int.Parse(list[len - 1]);

			for (int i = 0; i < retval.WannierFunctions.Count; i++)
			{
				retval.WannierFunctions[i].Data = new VolumeData(xgrid, ygrid, zgrid);
			}

			retval.Grid = new Grid();
			retval.Grid.GridSize[0] = xgrid;
			retval.Grid.GridSize[1] = ygrid;
			retval.Grid.GridSize[2] = zgrid;

			string[] text = SplitLine(ReadNextLine(reader));
			retval.Grid.Origin = Vector3.Parse(text[1], text[2], text[3]);
			
			for (int i = 0; i < 3; i++)
			{
				text = SplitLine(ReadNextLine(reader));

				retval.Grid.Delta[i] = Vector3.Parse(text[1], text[2], text[3]);
			}

			ReadToComment(reader);
		}

		private string[] SplitLine(string p)
		{
			return p.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}


		private void ReadWFdata(StreamReader reader, WannierData retval, int count, int number)
		{
			List<double> data = new List<double>();

			while (data.Count < count)
			{
				string line = ReadNextLine(reader);
				var thisdata = SplitLine(line).Select(x => double.Parse(x));
				data.AddRange(thisdata);
			}

			ReadToComment(reader);

			WannierFunction fn = retval.WannierFunctions[number];

			int index = 0;

			for (int x = 0; x < fn.Data.Width; x++)
			{
				for (int y = 0; y < fn.Data.Height; y++)
				{
					for (int z = 0; z < fn.Data.Depth; z++)
					{
						fn.Data[x, y, z] = data[index];

						index++;
					}
				}
			}
		}

		private void ReadJunkObject(StreamReader reader, WannierData retval, int count)
		{
			for (int i = 0; i < count; i++)
				ReadNextLine(reader);

			ReadNextLine(reader);
		}

		private static string StripQuotes(string val)
		{
			if (val.StartsWith("\"") == false || val.EndsWith("\"") == false)
				throw new Exception();

			string retval = val.Substring(1, val.Length - 2);

			return retval;
		}

		private void ReadWFnames(StreamReader reader, WannierData retval, int count)
		{
			while (retval.WannierFunctions.Count < count)
				retval.WannierFunctions.Add(new WannierFunction());

			for (int i = 0; i < count; i++)
			{
				string name = ReadNextLine(reader);

				if (name.StartsWith("\"") && name.EndsWith("\""))
					name = StripQuotes(name);

				retval.WannierFunctions[i].Name = name;
			}
		}

		private string[] GetMatches(string line)
		{
			var matches = reg.Matches(line);
			string[] list = matches.OfType<Match>().Select(x => x.ToString().ToLowerInvariant()).ToArray();
			return list;
		}

		const double bohrToAng = 0.52918;

		private void WriteData(WannierData data)
		{
			foreach (var wan in data.WannierFunctions)
			{
				string filename = string.Format("wan{0}.xsf", wan.Name);
				Grid grid = data.Grid;

				Console.WriteLine("Writing data to {0}", filename);

				using (StreamWriter wr = new StreamWriter(filename))
				{
					wr.WriteLine("CRYSTAL");
					wr.WriteLine("PRIMVEC");

					for (int i = 0; i < 3; i++)
					{
						Vector3 span = grid.SpanVectors[i] * bohrToAng;

						wr.WriteLine("{0}   {1}   {2}", span.X, span.Y, span.Z);
					}

					wr.WriteLine("PRIMCOORD");
					wr.WriteLine("{0}   1", data.Atoms.Count);
					foreach (var atom in data.Atoms)
					{
						Vector3 pos = atom.Position * bohrToAng;

						wr.WriteLine("{0}   {1}   {2}   {3}", atom.AtomicNumber, pos.X, pos.Y, pos.Z);
					}

					wr.WriteLine("BEGIN_BLOCK_DATAGRID3D");
					wr.WriteLine("Wannier");
					wr.WriteLine("BEGIN_DATAGRID_3D_Wannier");
					wr.WriteLine("{0}  {1}  {2}", grid.GridSize[0], grid.GridSize[1], grid.GridSize[2]);
					wr.WriteLine("{0}    {1}    {2}", grid.Origin.X, grid.Origin.Y, grid.Origin.Z);
					for (int i = 0; i < 3; i++)
					{
						Vector3 span = grid.SpanVectors[i] * bohrToAng;

						wr.WriteLine("{0}   {1}   {2}", span.X, span.Y, span.Z);
					}

					int count = 0;
					for (int k = 0; k < wan.Data.Depth; k++)
					{
						for (int j = 0; j < wan.Data.Height; j++)
						{
							for (int i = 0; i < wan.Data.Width; i++)
							{
								wr.Write("{0}   ", wan.Data[i, j, k]);

								count++;

								if (count == 3)
								{
									wr.WriteLine();
									count = 0;
								}
							}
						}
					}

					wr.WriteLine("END_DATAGRID_3D");
					wr.WriteLine("END_BLOCK_DATAGRID3D");
				}
			}
		}
	}
}
