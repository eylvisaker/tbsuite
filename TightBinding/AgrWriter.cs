using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	class AgrWriter: IDisposable 
	{
		StreamWriter file;

		public AgrWriter(string filename)
		{
			file = new StreamWriter(filename);
		}
		public void Dispose()
		{
			file.Dispose();
		}

		[Obsolete]
		public void WriteGraceDottedSetStyle(int index)
		{
			WriteGraceSetLineStyle(index, 2);
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

		public void WriteGraceHeader(KptList kpath)
		{
			file.WriteLine("@with g0");

			var pairs = kpath.Kpts.Select(
				(kpt, index) => new Pair<int, KPoint>(index, kpt)).ToArray();

			var pts = (
				from val in pairs
				where string.IsNullOrEmpty(val.Second.Name) == false
				select val
				).ToArray();


			file.WriteLine("@    xaxis  tick spec type both");
			file.WriteLine("@    xaxis  tick spec {0}", pts.Length);
			for (int i = 0; i < pts.Length; i++)
			{
				string label = pts[i].Second.Name;

				if (label.StartsWith("G"))
					label = @"\xG\f{}" + label.Substring(1);
				
				label = label.Replace("$_", @"\s");
				label = label.Replace("$^", @"\S");
				label = label.Replace("$.", @"\N");

				file.WriteLine("@    xaxis  tick major {0}, {1}", i, pts[i].First);
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


		public void WriteGraceDataset(int length, Func<int, double> data)
		{
			file.WriteLine("@type xy");
			for (int i = 0; i < length; i++)
			{
				file.WriteLine("{0}   {1}", i, data(i));
			}
			file.WriteLine("&");
		}
		public void WriteGraceDataset(string type, int length, Func<int, Pair<double, double>> data)
		{
			file.WriteLine("@type {0}", type);
			for (int i = 0; i < length; i++)
			{
				var val = data(i);
				if (val == null)
					continue;

				file.WriteLine("{0}   {1}   {2}", i, val.First, val.Second);
			}
			file.WriteLine("&");
		}

	}
}
