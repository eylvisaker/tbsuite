using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	class RpaAnalyze
	{
		static void Main(string[] args)
		{
			using (BootStrap b = new BootStrap())
			{
				string inputfile = b.GetInputFile("RPA Analysis code", "rpanal", args);

				TightBinding tb = new TightBinding(inputfile);
				RPA inst = new RPA();

				Vector3 q = GetQpoint();

				List<KPoint> qpt = new List<KPoint>();
				qpt.Add(new KPoint(q));

				var parameters = inst.CreateRpaParameterList(tb, qpt);
			}
		}

		private static Vector3 GetQpoint()
		{
			Console.Write("Enter q point to use: ");
			string text = Console.ReadLine();
			Vector3 q;

			if (Vector3.TryParse(text, out q) == false)
			{
				return GetQpoint();
			}

			return q;
		}
	}
}
