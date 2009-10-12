using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBinding
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

		public void WriteGraceDottedSetStyle(int index)
		{
			file.WriteLine("@    s{0} line linestyle 2", index);
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

				if (label == "G")
					label = @"\xG";

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

	}
}
