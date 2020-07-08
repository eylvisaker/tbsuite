using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace FploBandWeights
{
	class AgrWriter
	{

		public static void Write(string filename, IEnumerable<AgrDataset> data, List<SymmetryPoint> sympoints)
		{
			AgrWriter w = new AgrWriter(filename);

			w.WriteData(data.ToArray(),sympoints);

			w.Dispose();
		}


		StreamWriter file;

		AgrWriter(string filename)
		{
			file = new StreamWriter(filename);
		}

		void Dispose()
		{
			file.Dispose();
		}
		
		private void WriteData(AgrDataset[] agrDataset, List<SymmetryPoint> sympoints)
		{
			int index = 0;
			WriteGraceHeader(sympoints);

			foreach (var data in agrDataset)
			{
				WriteGraceSetLineColor(index, (int)data.LineColor);
				WriteGraceSetLineStyle(index, (int)data.LineStyle);
				WriteGraceSetSymbol(index, (int)data.Symbol);
				WriteGraceSetSymbolColor(index, (int)data.SymbolColor);
				WriteGraceSetSymbolFill(index, (int)data.SymbolFill);
				WriteGraceLegend(index, data.Legend);

				index++;
			}

			index = 0;

			foreach (var data in agrDataset)
			{
				if (data.DatasetType == DatasetType.xy)
				{
					WriteGraceDataset(data.Data.Count,
									x => new Pair<double, double>(data.Data[x].X, data.Data[x].Y));
				}
				else 
				{
					WriteGraceDataset(data.DatasetType.ToString(), data.Data.Count,
						x => new Triplet<double, double, double>(data.Data[x].X, data.Data[x].Y, data.Data[x].Weight));
				}

				index++;
			}
		}

		public void WriteGraceSetLineStyle(int index, int linestyle)
		{
			file.WriteLine("@    s{0} line linestyle {1}", index, linestyle);
		}

		public void WriteGraceLegend(int dataset, string text)
		{
			file.WriteLine("@    s{0} legend \"{1}\"", dataset, text);
		}
		public void WriteGraceSetLineColor(int start, params int[] linecolor)
		{
			for (int i = 0; i < linecolor.Length; i++)
			{
				file.WriteLine("@    s{0} line linewidth 2.0", i + start);
				file.WriteLine("@    s{0} line color {1}", i + start, linecolor[i]);
			}
		}
		public void WriteGraceSetSymbol(int set, int symbol)
		{
			file.WriteLine("@    s{0} symbol {1}", set, symbol);
		}
		public void WriteGraceSetSymbolColor(int set, int symbolcolor)
		{
			file.WriteLine("@    s{0} symbol color {1}", set, symbolcolor);
			file.WriteLine("@    s{0} symbol fill color {1}", set, symbolcolor);
		}
		public void WriteGraceSetSymbolFill(int set, int symbolfill)
		{
			file.WriteLine("@    s{0} symbol fill pattern {1}", set, symbolfill);
		}

		public void WriteGraceHeader(List<SymmetryPoint> symPoints)
		{
			file.WriteLine("@with g0");

			file.WriteLine("@    xaxis  tick spec type both");
			file.WriteLine("@    xaxis  tick spec {0}", symPoints.Count);

			for (int i = 0; i < symPoints.Count; i++)
			{
				string label = symPoints[i].Name;

				label = label.Replace("$_", @"\s");
				label = label.Replace("$^", @"\S");
				label = label.Replace("$.", @"\N");

				file.WriteLine("@    xaxis  tick major {0}, {1}", i, symPoints[i].Location);
				file.WriteLine("@    xaxis  ticklabel {0}, \"{1}\"", i, label);
			}

		}

		public void WriteGraceBaseline(int length)
		{
			file.WriteLine("@type xy");
			file.WriteLine("0 0");
			file.WriteLine("{0} 0", length);
			file.WriteLine("&");
		}


		public void WriteGraceDataset(int length, Func<int, Pair<double,double>> data)
		{
			file.WriteLine("@type xy");
			for (int i = 0; i < length; i++)
			{
				var val = data(i);

				file.WriteLine("{0}   {1}", val.First, val.Second);
			}
			file.WriteLine("&");
		}
		public void WriteGraceDataset(string type, int length, Func<int, Triplet<double, double,double>> data)
		{
			file.WriteLine("@type {0}", type);
			for (int i = 0; i < length; i++)
			{
				var val = data(i);
				if (val == null)
					continue;

				file.WriteLine("{0}   {1}   {2}", val.First, val.Second, val.Third);
			}
			file.WriteLine("&");
		}

	}
}
